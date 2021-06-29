using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;


#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif


namespace eDmitriyAssets.NavmeshLinksGenerator
{

    public class NavMeshLinks_AutoPlacer : MonoBehaviour
    {
        #region Variables

        public Transform linkPrefab;
        public Transform onewayLinkPrefab;

        public NavMeshSurface surface;

        public GameObject sphereCast;

        public GenerateGame generateGame;

        //public GameObject cube;
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
        public bool dontAllignYAxis = false;


        //private List< Vector3 > spawnedLinksPositionsList = new List< Vector3 >();
        private Mesh currMesh;
        private List<Edge> edges = new List<Edge>();

        private float agentRadius = 2;

        private Vector3 ReUsableV3;
        private Vector3 offSetPosY;

        #endregion






        #region GridGen

        public void Generate()
        {
            if (linkPrefab == null) return;
            agentRadius = NavMesh.GetSettingsByIndex(0).agentRadius;
            edges.Clear();
            //spawnedLinksPositionsList.Clear();
            // calculates edges perfectly, do not understand alignment or need for inversion
            CalcEdges();

            PlaceTiles();


#if UNITY_EDITOR
            if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif

        }

        public void Start()
        {
            PlaceGame();
        }

        public void PlaceGame()
        {
            generateGame.CreateGame(Constants.playerType);
            surface.RemoveData();
            surface.BuildNavMesh();
            Generate();
        }

        public GameObject PlaceGame(Vector3 plane, Quaternion orientation, BlockType playerType)
        {
            GameObject game = generateGame.CreateGame(plane, orientation, playerType);
            surface.RemoveData();
            surface.BuildNavMesh();
            Generate();
            return game;
        }

        public void ClearLinks()
        {
            List<NavMeshLink> navMeshLinkList = GetComponentsInChildren<NavMeshLink>().ToList();
            while (navMeshLinkList.Count > 0)
            {
                GameObject obj = navMeshLinkList[0].gameObject;
                if (obj != null) DestroyImmediate(obj);
                navMeshLinkList.RemoveAt(0);
            }
        }

        private void PlaceTiles()
        {
            if (edges.Count == 0) return;

            ClearLinks();

            foreach (Edge edge in edges)
            {
                //Debug.DrawLine(edge.start + Vector3.up * 0.5f, edge.end + Vector3.up * 0.5f, Color.green, 15);

                // Edge length is 0.4 all around apart from a count which are 0.4000001
                Debug.Log(edge.length);
                // this is clamped to 0 because 0.4 is a float and this is casted into int
                int tilesCountWidth = (int)Mathf.Clamp(edge.length / tileWidth, 0, 10000);

                float heightShift = 0;

                //if (edge.length > 0.41 && edge.length < 0.5)
                //{
                //    Debug.DrawLine(edge.start + Vector3.up * 0.5f, edge.end + Vector3.up * 0.5f, Color.red, 15);
                //} 
                //if (tilesCountWidth >= 2)
                //{
                //    Debug.DrawLine(edge.start + Vector3.up * 0.5f, edge.end + Vector3.up * 0.5f, Color.green, 15);
                //}

                //Debug.DrawLine(edge.start + Vector3.right * 0.2f, edge.end, Color.green, 15);

                //Debug.Log($"tilesCountWidth: {tilesCountWidth}");

                // iterate through
                for (int columnN = 0; columnN < tilesCountWidth; columnN++) //every edge length segment
                {
                    Vector3 placePos = Vector3.Lerp(
                                           edge.start,
                                           edge.end,
                                           (float)columnN / (float)tilesCountWidth //position on edge
                                           + 0.5f / (float)tilesCountWidth //shift for half tile width
                                       ) + edge.facingNormal * Vector3.up * heightShift;

                    //Debug.DrawLine(placePos, placePos + Vector3.down * 3 , Color.green, 40);

                    //spawn up/down links
                    CheckPlacePos(placePos, edge.facingNormal);
                    //spawn horizontal links
                    CheckPlacePosHorizontal(placePos, edge.facingNormal);
                }
            }
        }

        bool CheckPlacePos(Vector3 pos, Quaternion normal)
        {
            bool result = false;

            Vector3 startPos = pos + normal * Vector3.forward * agentRadius * 2;
            Vector3 endPos = startPos - Vector3.up * maxJumpHeight * 1.1f;

            // create line from navmesh downwards
            //Debug.DrawLine(pos + Vector3.right * 0.2f, endPos, Color.white, 15);
            //Debug.DrawLine(pos, endPos, Color.white, 15);

            NavMeshHit navMeshHit;
            RaycastHit raycastHit = new RaycastHit();

            // ignore trigger colliders during raycast
            if (Physics.Linecast(startPos, endPos, out raycastHit, raycastLayerMask.value,
                QueryTriggerInteraction.Ignore))
            {
                if (NavMesh.SamplePosition(raycastHit.point, out navMeshHit, 1f, NavMesh.AllAreas))
                {
                    //Debug.DrawLine(pos, navMeshHit.position, Color.green, 15);

                    if (Vector3.Distance(pos, navMeshHit.position) > 1.1f)
                    {
                        //added these 2 line to check to make sure there aren't flat horizontal links going through walls
                        Vector3 calcV3 = (pos - normal * Vector3.forward * 0.02f);
                        if ((calcV3.y - navMeshHit.position.y) > 1f)
                        {

                            //SPAWN NAVMESH LINKS
                            Transform spawnedTransf = Instantiate(
                                linkPrefab.transform,
                                //pos - normal * Vector3.forward * 0.02f,
                                calcV3,
                                normal
                            ) as Transform;

                            var nmLink = spawnedTransf.GetComponent<NavMeshLink>();
                            nmLink.startPoint = Vector3.zero;
                            nmLink.endPoint = nmLink.transform.InverseTransformPoint(navMeshHit.position);
                            nmLink.UpdateLink();

                            spawnedTransf.SetParent(transform);
                        }
                    }
                }
            }

            return result;
        }

        bool CheckPlacePosHorizontal(Vector3 pos, Quaternion normal)
        {
            bool result = false;

            Vector3 startPos = pos + normal * Vector3.forward * agentRadius * 2;
            Vector3 endPos = startPos - normal * Vector3.back * maxJumpDist * 1.1f;

            // Cheat forward a little bit so the sphereCast doesn't touch this ledge.
            // this has to be set so that the horizontal cast line goes from ledge directly across
            Vector3 cheatStartPos = LerpByDistance(startPos, endPos, cheatOffset);

            //Debug.DrawRay(endPos, Vector3.up, Color.blue, 20);
            //Debug.DrawLine(cheatStartPos, endPos, Color.white, 20);
            //Debug.DrawLine(startPos, endPos, Color.white, 2);


            NavMeshHit navMeshHit;
            RaycastHit raycastHit = new RaycastHit();

            //calculate direction for Spherecast
            ReUsableV3 = endPos - startPos;
            // raise up pos Y value slightly up to check for wall/obstacle
            offSetPosY = new Vector3(pos.x, (pos.y + wallCheckYOffset), pos.z);

            //Debug.DrawLine(pos, offSetPosY, Color.white, 20);

            // ray cast to check for walls
            if (!Physics.Raycast(offSetPosY, ReUsableV3, (maxJumpDist / 2), raycastLayerMask.value))
            {
                //Debug.DrawRay(offSetPosY, ReUsableV3, Color.yellow, 15);
                //Debug.DrawRay(pos, ReUsableV3, Color.yellow, 15);
                Vector3 ReverseRayCastSpot = (offSetPosY + (ReUsableV3));
                //now raycast back the other way to make sure we're not raycasting through the inside of a mesh the first time.
                if (!Physics.Raycast(ReverseRayCastSpot, -ReUsableV3, (maxJumpDist + 1), raycastLayerMask.value))
                {
                    //Debug.DrawRay(ReverseRayCastSpot, -ReUsableV3, Color.red, 25);

                    //if no walls 1 unit out then check for other colliders using the Cheat offset so as to not detect the edge we are spherecasting from.
                    if (Physics.SphereCast(cheatStartPos, sphereCastRadius, ReUsableV3, out raycastHit, maxJumpDist, raycastLayerMask.value))
                                                //if (Physics.Linecast(startPos, endPos, out raycastHit, raycastLayerMask.value, QueryTriggerInteraction.Ignore))
                    {
                        //Debug.DrawRay(cheatStartPos, cheatStartPos + sphereCastRadius * Vector3.up, Color.red, 25);
                        //GameObject go = Instantiate(sphereCast);
                        //go.transform.position = cheatStartPos;
                        //go.transform.localScale = new Vector3(sphereCastRadius * 2, sphereCastRadius * 2, sphereCastRadius * 2);

                        Vector3 cheatRaycastHit = LerpByDistance(raycastHit.point, endPos, .2f);
                        // some of these links are going through mesh
                        if (NavMesh.SamplePosition(cheatRaycastHit, out navMeshHit, 1f, NavMesh.AllAreas))
                        {
                            //Debug.Log("Success");

                            NavMeshPath path = new NavMeshPath();

                            NavMesh.CalculatePath(pos, navMeshHit.position, NavMesh.AllAreas, path);
                            if (path.status != NavMeshPathStatus.PathComplete && !Physics.Raycast(pos, navMeshHit.position) &&
                                Vector3.Distance(pos, navMeshHit.position) > 1.1f)
                            {
                                //Debug.DrawLine(pos, navMeshHit.position, Color.red, 25);
                                //SPAWN NAVMESH LINKS
                                Transform spawnedTransf = Instantiate(
                                    onewayLinkPrefab.transform,
                                    pos - normal * Vector3.forward * 0.02f,
                                    normal
                                ) as Transform;

                                var nmLink = spawnedTransf.GetComponent<NavMeshLink>();
                                nmLink.startPoint = Vector3.zero;
                                nmLink.endPoint = nmLink.transform.InverseTransformPoint(navMeshHit.position);
                                nmLink.UpdateLink();

                                spawnedTransf.SetParent(transform);
                            }
                            // should check if the y position is within a range. if navMeshHit.position.y < pos.y (angle wise) then
                            // instantiate onewayLink. Else instantiate two way link

                            //if (Vector3.Distance(pos, navMeshHit.position) > 1.1f)
                            //{
                            //    //SPAWN NAVMESH LINKS
                            //    Transform spawnedTransf = Instantiate(
                            //        onewayLinkPrefab.transform,
                            //        pos - normal * Vector3.forward * 0.02f,
                            //        normal
                            //    ) as Transform;

                            //    var nmLink = spawnedTransf.GetComponent<NavMeshLink>();
                            //    nmLink.startPoint = Vector3.zero;
                            //    nmLink.endPoint = nmLink.transform.InverseTransformPoint(navMeshHit.position);
                            //    nmLink.UpdateLink();

                            //    spawnedTransf.SetParent(transform);
                            //}
                        }
                    }
                }
            }
            return result;
        }

        #endregion
        //Just a helper function I added to calculate a point between normalized distance of two V3s
        public Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
        {
            Vector3 P = x * Vector3.Normalize(B - A) + A;
            return P;
        }


        #region EdgeGen


        float triggerAngle = 0.999f;

        private void CalcEdges()
        {
            // extracts navmesh into a normal mesh
            var tr = NavMesh.CalculateTriangulation();
            currMesh = new Mesh()
            {
                vertices = tr.vertices,
                triangles = tr.indices
            };

            for (int i = 0; i < currMesh.triangles.Length - 1; i += 3)
            {
                //CALC FROM MESH OPEN EDGES vertices

                TrisToEdge(currMesh, i, i + 1);
                TrisToEdge(currMesh, i + 1, i + 2);
                TrisToEdge(currMesh, i + 2, i);
            }


            // here are the open edges of the meshes created from triangulation
            foreach (Edge edge in edges)
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

                    //Never gets triggered: NOT SURE WHY
                    if (edge.startUp.sqrMagnitude > 0)
                    {
                        var vect = Vector3.Lerp(edge.endUp, edge.startUp, 0.5f) -
                                   Vector3.Lerp(edge.end, edge.start, 0.5f);
                        edge.facingNormal = Quaternion.LookRotation(Vector3.Cross(edge.end - edge.start, vect));


                        //FIX FOR NORMALs POINTING DIRECT TO UP/DOWN
                        if (Mathf.Abs(Vector3.Dot(Vector3.up, (edge.facingNormal * Vector3.forward).normalized)) >
                            triggerAngle)
                        {
                            edge.startUp += new Vector3(0, 0.1f, 0);
                            vect = Vector3.Lerp(edge.endUp, edge.startUp, 0.5f) -
                                   Vector3.Lerp(edge.end, edge.start, 0.5f);
                            edge.facingNormal = Quaternion.LookRotation(Vector3.Cross(edge.end - edge.start, vect));
                        }
                    }

                    // I do not know what the difference is between dont align and align
                    // so i am commenting it out 
                    //if (dontAllignYAxis)
                    //{
                    //    edge.facingNormal = Quaternion.LookRotation(
                    //        edge.facingNormal * Vector3.forward,
                    //        Quaternion.LookRotation(edge.end - edge.start) * Vector3.up
                    //    );
                    //}

                    edge.facingNormalCalculated = true;


                }

                if (invertFacingNormal) edge.facingNormal = Quaternion.Euler(Vector3.up * 180) * edge.facingNormal;

                //Debug.DrawLine(edge.facingNormal * Vector3.forward, Vector3.forward * 10, Color.green, 15);

                //Debug.DrawLine(edge.start + Vector3.up * 0.5f, edge.end + Vector3.up * 0.5f, Color.green, 15);

            }
        }


        private void TrisToEdge(Mesh currMesh, int n1, int n2)
        {
            Vector3 val1 = currMesh.vertices[currMesh.triangles[n1]];
            Vector3 val2 = currMesh.vertices[currMesh.triangles[n2]];

            Edge newEdge = new Edge(val1, val2);

            //remove duplicate edges
            foreach (Edge edge in edges)
            {
                if ((edge.start == val1 & edge.end == val2)
                    || (edge.start == val2 & edge.end == val1)
                )
                {
                    //print("Edges duplicate " + newEdge.start + " " + newEdge.end);
                    edges.Remove(edge);
                    return;
                }
            }

            edges.Add(newEdge);
        }

        #endregion
    }



    [Serializable]
    public class Edge
    {
        public Vector3 start;
        public Vector3 end;

        public Vector3 startUp;
        public Vector3 endUp;

        public float length;
        public Quaternion facingNormal;
        public bool facingNormalCalculated = false;


        public Edge(Vector3 startPoint, Vector3 endPoint)
        {
            start = startPoint;
            end = endPoint;
        }
    }





#if UNITY_EDITOR

    [CustomEditor(typeof(NavMeshLinks_AutoPlacer))]
    [CanEditMultipleObjects]
    public class NavMeshLinks_AutoPlacer_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Generate"))
            {
                foreach (var targ in targets)
                {
                    ((NavMeshLinks_AutoPlacer)targ).Generate();
                }
            }

            if (GUILayout.Button("ClearLinks"))
            {
                foreach (var targ in targets)
                {
                    ((NavMeshLinks_AutoPlacer)targ).ClearLinks();
                }
            }
        }
    }

#endif
}
