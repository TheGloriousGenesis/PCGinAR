using System.Collections.Generic;
using System.Linq;
using Behaviour.Platform.LinkGenerator.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

#if UNITY_EDITOR

#endif


namespace Behaviour.Platform.LinkGenerator
{
    public class NavMeshLinksAutoPlacer : MonoBehaviour
    {
        #region Variables

        public Transform linkPrefab;
        public Transform onewayLinkPrefab;

        public NavMeshSurface surface;
        
        public float tileWidth = 5f;

        [Header("OffMeshLinks")]
        public float maxJumpHeight = 3f;
        public float maxJumpDist = 5f;
        public LayerMask raycastLayerMask = -1;
        public float sphereCastRadius = 1f;

        //how far over to move spherecast away from navmesh edge to prevent detecting the same edge
        public float cheatOffset = .25f;

        //how high up to bump raycasts to check for walls (to prevent forming links through walls)
        public float wallCheckYOffset = 0.5f;

        [Header("EdgeNormal")] public bool invertFacingNormal = false;
        public bool DontAlignYAxis { get; } = false;


        private Mesh _currMesh;
        private readonly List<Edge> _edges = new List<Edge>();
        private readonly List<Vector3> _edgeVertices = new List<Vector3>();

        private float _agentRadius = 2;

        private Vector3 _reUsableV3;
        private Vector3 _offSetPosY;

        #endregion
        
        #region GridGen
        public void Generate()
        {
            if (linkPrefab == null) return;
            _agentRadius = NavMesh.GetSettingsByIndex(0).agentRadius;
            _edges.Clear();
            // calculates edges perfectly, do not understand alignment or need for inversion
            CalcEdges();

            PlaceTiles();

//todo: put this back in if you editing game, remove for deploying to mobile
// #if UNITY_EDITOR
//             if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(gameObject.scene);
// #endif

        }

        public void Start()
        {
            RefreshLinks();
        }

        public void RefreshLinks()
        {
            surface.RemoveData();
            surface.BuildNavMesh();
            // Generate();
        }

        public static NavMeshPath ContainsPath()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            var target = GameObject.FindGameObjectWithTag("Goal");
            var path = new NavMeshPath();
            NavMesh.CalculatePath(player.transform.position, target.transform.position, NavMesh.AllAreas, path);
            return path;
        }

        public void ClearLinks()
        {
            var navMeshLinkList = GetComponentsInChildren<NavMeshLink>().ToList();
            while (navMeshLinkList.Count > 0)
            {
                var obj = navMeshLinkList[0].gameObject;
                if (obj != null) DestroyImmediate(obj);
                navMeshLinkList.RemoveAt(0);
            }
        }

        public void ClearSurfaceData()
        {
            surface.RemoveData();
        }

        private void PlaceTiles()
        {
            if (_edges.Count == 0) return;

            ClearLinks();

            foreach (var edge in _edges)
            {
                // this is clamped to 0 because 0.4 is a float and this is casted into int
                var tilesCountWidth = (int)Mathf.Clamp(edge.length / tileWidth, 0, 10000);

                const float heightShift = 0;

                // iterate through
                for (var columnN = 0; columnN < tilesCountWidth; columnN++) //every edge length segment
                {
                    var placePos = Vector3.Lerp(
                                           edge.start,
                                           edge.end,
                                           (float)columnN / (float)tilesCountWidth //position on edge
                                           + 0.5f / (float)tilesCountWidth //shift for half tile width
                                       ) + edge.facingNormal * Vector3.up * heightShift;
                    
                    //spawn up/down links
                    CheckPlacePos(placePos, edge.facingNormal);
                    //spawn horizontal links
                    CheckPlacePosHorizontal(placePos, edge.facingNormal);
                }
            }
        }

        private void CheckPlacePos(Vector3 pos, Quaternion normal)
        {
            var startPos = pos + normal * Vector3.forward * _agentRadius * 2;
            var endPos = startPos - Vector3.up * maxJumpHeight * 1.1f;

            // ignore trigger colliders during raycast
            if (!Physics.Linecast(startPos, endPos, out var raycastHit, raycastLayerMask.value,
                QueryTriggerInteraction.Ignore)) return;
            if (!NavMesh.SamplePosition(raycastHit.point, out var navMeshHit, 1f, NavMesh.AllAreas)) return;
            var path = new NavMeshPath();
            NavMesh.CalculatePath(pos, navMeshHit.position, NavMesh.AllAreas, path);

            var dis = Vector3.Distance(pos, navMeshHit.position);
            var calcV3 = (pos - normal * Vector3.forward * 0.02f);
            var extremeDrop = calcV3.y - navMeshHit.position.y;

            if (path.status == NavMeshPathStatus.PathComplete) return;
            if (dis > 1.1f && (extremeDrop < maxJumpHeight))
            {
                //SPAWN NAVMESH LINKS
                var spawnedTransform = Instantiate(
                    linkPrefab.transform,
                    calcV3,
                    normal
                );

                var nmLink = spawnedTransform.GetComponent<NavMeshLink>();
                nmLink.startPoint = Vector3.zero;
                nmLink.endPoint = nmLink.transform.InverseTransformPoint(navMeshHit.position);
                nmLink.UpdateLink();

                spawnedTransform.SetParent(transform);
            } else if (dis > 1.1f && (extremeDrop > maxJumpHeight))
            {
                //SPAWN NAVMESH LINKS
                var spawnedTransform = Instantiate(
                    onewayLinkPrefab.transform,
                    calcV3,
                    normal
                );

                var nmLink = spawnedTransform.GetComponent<NavMeshLink>();
                nmLink.startPoint = Vector3.zero;
                nmLink.endPoint = nmLink.transform.InverseTransformPoint(navMeshHit.position);
                nmLink.UpdateLink();

                spawnedTransform.SetParent(transform);
            }
        }

        private void CheckPlacePosHorizontal(Vector3 pos, Quaternion normal)
        {
            var startPos = pos + normal * Vector3.forward * _agentRadius * 2;
            var endPos = startPos - normal * Vector3.back * maxJumpDist * 1.1f;

            // Cheat forward a little bit so the sphereCast doesn't touch this ledge.
            // this has to be set so that the horizontal cast line goes from ledge directly across
            var cheatStartPos = LerpByDistance(startPos, endPos, cheatOffset);

            //calculate direction for Spherecast
            _reUsableV3 = endPos - startPos;
            // raise up pos Y value slightly up to check for wall/obstacle
            _offSetPosY = new Vector3(pos.x, (pos.y + wallCheckYOffset), pos.z);

            // todo: before there were checks to make sure links were not made in mesh. check to see if this happens in this change:
            
            // not sure why we use sphere cast radius and not jump distance ==> this is because the radius of the sphere cut after maxJumpDist
            if (!Physics.SphereCast(cheatStartPos, sphereCastRadius, _reUsableV3, out var raycastHit, maxJumpDist,
                raycastLayerMask.value))
                return;
            var cheatRaycastHit = LerpByDistance(raycastHit.point, endPos, .2f);
            // some of these links are going through mesh
            if (!NavMesh.SamplePosition(cheatRaycastHit, out var navMeshHit, 1f, NavMesh.AllAreas)) return;
            // Do not make links on navmeshes that are already connected so check path from pos to navmesh hit
            var path = new NavMeshPath();
            NavMesh.CalculatePath(pos, navMeshHit.position, NavMesh.AllAreas, path);
            var dis = Vector3.Distance(pos, navMeshHit.position);
            if (path.status == NavMeshPathStatus.PathComplete || Physics.Raycast(pos, navMeshHit.position) ||
                (!(dis > 1.1f)))
                return;
            //SPAWN NAVMESH LINKS
            var spawnedTransform = Instantiate(
                linkPrefab.transform,
                pos - normal * Vector3.forward * 0.02f,
                normal
            );

            var nmLink = spawnedTransform.GetComponent<NavMeshLink>();
            nmLink.startPoint = Vector3.zero;
            nmLink.endPoint = nmLink.transform.InverseTransformPoint(navMeshHit.position);
            nmLink.UpdateLink();

            spawnedTransform.SetParent(transform);
        }
        #endregion
        
        //Just a helper function I added to calculate a point between normalized distance of two V3s
        private static Vector3 LerpByDistance(Vector3 a, Vector3 b, float x)
        {
            Vector3 p = x * Vector3.Normalize(b - a) + a;
            return p;
        }

        #region EdgeGen
        private void CalcEdges()
        {
            // extracts navmesh into a normal mesh
            var tr = NavMesh.CalculateTriangulation();
            _currMesh = new Mesh()
            {
                vertices = tr.vertices,
                triangles = tr.indices
            };

            for (int i = 0; i < _currMesh.triangles.Length - 1; i += 3)
            {
                //CALC FROM MESH OPEN EDGES vertices
                TrisToEdge(_currMesh, i, i + 1);
                TrisToEdge(_currMesh, i + 1, i + 2);
                TrisToEdge(_currMesh, i + 2, i);
            }

            Utility.EdgesOfCurrentGame = _edgeVertices;

            // here are the open edges of the meshes created from triangulation
            foreach (var edge in _edges)
            {
                //EDGE LENGTH
                edge.length = Vector3.Distance(
                    edge.start,
                    edge.end
                );

                //FACING NORMAL
                if (!edge.facingNormalCalculated)
                {
                    edge.facingNormal = Quaternion.LookRotation(Vector3.Cross(edge.end - edge.start, Vector3.up));

                    edge.facingNormalCalculated = true;
                    
                }

                if (invertFacingNormal) edge.facingNormal = Quaternion.Euler(Vector3.up * 180) * edge.facingNormal;
            }
        }


        private void TrisToEdge(Mesh currMesh, int n1, int n2)
        {
            var val1 = currMesh.vertices[currMesh.triangles[n1]];
            var val2 = currMesh.vertices[currMesh.triangles[n2]];

            var newEdge = new Edge(val1, val2);

            //remove duplicate edges
            foreach (var edge in _edges.Where(edge => (edge.start == val1 & edge.end == val2)
                                                      || (edge.start == val2 & edge.end == val1)))
            {
                _edges.Remove(edge);
                return;
            }
            _edges.Add(newEdge);
            _edgeVertices.Add(val1);
            _edgeVertices.Add(val2);
        }
        #endregion
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(NavMeshLinksAutoPlacer))]
    [CanEditMultipleObjects]
    public class NavMeshLinksAutoPlacer_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Generate"))
            {
                foreach (var targ in targets)
                {
                    ((NavMeshLinksAutoPlacer)targ).Generate();
                }
            }

            if (!GUILayout.Button("ClearLinks")) return;
            {
                foreach (var targ in targets)
                {
                    ((NavMeshLinksAutoPlacer)targ).ClearLinks();
                }
            }
        }
    }

#endif
}
