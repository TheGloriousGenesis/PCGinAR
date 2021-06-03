//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.XR.ARFoundation;
//using UnityEngine.XR.ARSubsystems;
//using BasicGeneticAlgorithmNS;
//using BaseGeneticClass;
//using System.Linq;

///// <summary>
///// tutorial to do this at:
///// https://www.youtube.com/watch?v=xguiSueY1Lw
///// </summary>

//// Required to enforce the type of object needed for placement
//// automatically gets pulled in onto any object using this code
//[RequireComponent(typeof(ARRaycastManager))]
//[RequireComponent(typeof(ARPlaneManager))]
//public class TapToPlace : MonoBehaviour
//{
//    private GameObject spawnedObject;
//    private ARRaycastManager _arRaycastManager;
//    private ARPlaneManager _arPlaneManager;
//    static List<ARRaycastHit> hits = new List<ARRaycastHit>();
//    private BasicGeneticAlgorithm bga = new BasicGeneticAlgorithm();


//    public PrefabFactory prefabs;
//    public BlockTile[] blockTiles;

//    public int platformLength;
//    private void Awake()
//    {
//        _arRaycastManager = GetComponent<ARRaycastManager>();
//        _arPlaneManager = GetComponent<ARPlaneManager>();
//    }

//    // what is 'out' in this?
//    bool TryGetTouchPosition(out Vector2 touchPosition)
//    {
//        if (Input.touchCount > 0)
//        {
//            touchPosition = Input.GetTouch(0).position;
//            return true;
//        }

//        touchPosition = default;
//        return false;
//    }

//    private void Update()
//    {
//        if (!TryGetTouchPosition(out Vector2 touchPosition))
//            return;
//        // why do we need hits?
//        // TrackableType : feature in physicall environment that a device is able to track like a plane
//        if (_arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
//        {
//            var hitPose = hits[0].pose;

//            Quaternion orientation = Quaternion.identity;
            
//            GetCameraAlignment(hits[0], out orientation);

//            // if there is object, move around. If not then spawn
//            if (spawnedObject == null)
//            {
//                Chromosone solution = bga.GenerateChromosome(10);
//                CreatePlatform(solution, hitPose.position, orientation);
//                //CreatePlatform(hitPose.position, orientation);
//                //PlacePlayer(hitPose.position, orientation);
//                spawnedObject = GameObject.Find("Platform");
//            }
//            else
//            {
//                spawnedObject.transform.position = hitPose.position;
//            }
//        }
//    }


//    private void CreatePlatform(Vector3 startPosition, Quaternion startRotation)
//    {
//        Vector3 blockSize = prefabs[BlockType.BASICBLOCK].transform.localScale;

//        // this method will instantiate platform created via GA or EA
//        float count = 0;
//        while (count < platformLength)
//        {
//            GameObject newObj = Instantiate(prefabs[BlockType.BASICBLOCK], startPosition + new Vector3(count, 0, 0), startRotation);
//            newObj.transform.parent = GameObject.Find("Platform").transform;
//            count = count + blockSize.x;
//        }
//    }

//    private void CreatePlatform(Chromosone chromosone, Vector3 plane, Quaternion orientation)
//    {
//        Vector3 blockSize = prefabs[BlockType.BASICBLOCK].transform.localScale;
//        float count = 0;

//        List<Gene> genes = chromosone.genes;
//        List<int[]> position = genes.SelectMany(x => x.allele_blockPositions).ToList();

//        foreach (int[] i in position)
//        {
//            GameObject block = Instantiate(prefabs[BlockType.BASICBLOCK], new Vector3(i[0], i[1], i[2]), orientation);
//            block.transform.parent = GameObject.Find("Platform").transform;
//            count = count + blockSize.x;
//        }

//        // place object on first brick in lists
//        int[] firstInList = position[0];

//        PlacePlayer(new Vector3(firstInList[0], firstInList[1], firstInList[2]), orientation);

//        GameObject.Find("Platform").transform.position = plane;

//    }
//    private void PlacePlayer(Vector3 position, Quaternion rotation)
//    {
//        // for now move player with platform. Later change this so that platform created first. locked. then player placed
//        GameObject player =  Instantiate(prefabs[BlockType.PLAYER], position + new Vector3(0, 7, 0), rotation);
//        player.transform.parent = GameObject.Find("Platform").transform;
//    }


//    private void GetCameraAlignment(ARRaycastHit _planeHit, out Quaternion orientation)
//    {
//        TrackableId planeHit_ID = _planeHit.trackableId;
//        ARPlane planeHit = _arPlaneManager.GetPlane(planeHit_ID);
//        Vector3 planeNormal = planeHit.normal;
//        orientation = Quaternion.FromToRotation(Vector3.up, planeNormal);
//    }
//}
