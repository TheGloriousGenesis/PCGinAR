using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using BasicGeneticAlgorithmNS;
using BaseGeneticClass;
using System.Linq;
using System;


[RequireComponent(typeof(ARRaycastManager))]
public class PlacementDragAndLock : MonoBehaviour
{
    [SerializeField]
    private GameObject placedPrefab;

    [SerializeField]
    private Button lockButton;

    [SerializeField]
    private Button generateButton;

    [SerializeField]
    private Camera arCamera;

    [SerializeField]
    private float defaultRotation = 0;

    private GameObject placedObject;

    private Vector2 touchPosition = default;

    private ARRaycastManager arRaycastManager;

    private bool isLocked = false;

    // know if we touching screen
    private bool onTouchHold = false;

    // kknow what objects we touching
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private BasicGeneticAlgorithm bga = new BasicGeneticAlgorithm();

    public PrefabFactory prefabs;

    public BlockTile[] blockTiles;

    private float initialDistance;

    private Vector3 initialScale;

    private Chromosone currentSolution;

    void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();

        if (lockButton != null)
        {
            // when some one presses button call method (in brackets)
            lockButton.onClick.AddListener(Lock);
        }

        if (generateButton != null)
        {
            // when some one presses button call method (in brackets)
            Debug.Log("Enters generateButton Method");
            Debug.Log("Is placed object null: " + (placedObject == null));
            generateButton.onClick.AddListener(delegate { CreatePlatform(placedObject.transform.position, placedObject.transform.rotation); });
            Debug.Log("Finishes generate Button method");
        }
    }
    private void Lock()
    {
        isLocked = !isLocked;
        lockButton.GetComponentInChildren<Text>().text = isLocked ? "Locked" : "Unlocked";
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            touchPosition = touch.position;

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = arCamera.ScreenPointToRay(touch.position);
                RaycastHit hitObject;
                if (Physics.Raycast(ray, out hitObject))
                {
                    if (placedPrefab != null)
                    {
                        onTouchHold = isLocked ? false : true;
                    }
                }
            }

            if (touch.phase == TouchPhase.Ended)
            {
                onTouchHold = false;
            }
        }

        MoveGameObject();

        PinchAndZoom();
    }

    private void MoveGameObject()
    {
        if (arRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            if (placedObject == null)
            {
                if (defaultRotation > 0)
                {
                    // changes rotation to camera
                    placedObject = CreatePlatform(hitPose.position, Quaternion.identity);
                    placedObject.transform.Rotate(Vector3.up, defaultRotation);
                }
                else
                {
                    placedObject = CreatePlatform(hitPose.position, hitPose.rotation);
                }
            }
            else
            {
                if (onTouchHold)
                {
                    placedObject.transform.position = hitPose.position;
                    if (defaultRotation == 0)
                    {
                        placedObject.transform.rotation = hitPose.rotation;
                    }
                }
            }
        }
    }

    private void PinchAndZoom()
    {
        if (Input.touchCount == 2)
        {
            var touchZero = Input.GetTouch(0);
            var touchOne = Input.GetTouch(1);

            if (touchZero.phase == TouchPhase.Ended || touchZero.phase == TouchPhase.Canceled ||
                touchOne.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Canceled)
            {
                return;
            }

            if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(touchZero.position, touchOne.position);
                initialScale = placedObject.transform.localScale;

            }
            else
            {
                var currentDistance = Vector2.Distance(touchZero.position, touchOne.position);

                if (Mathf.Approximately(initialDistance, 0))
                {
                    return; //change is too small to do anything
                }

                var factor = currentDistance / initialDistance;
                placedObject.transform.localScale = initialScale * factor;
            }

        }
    }

    private GameObject CreatePlatform(Vector3 plane, Quaternion orientation)
    {
        Chromosone chromosone = bga.GenerateChromosome(4);
        currentSolution = chromosone;

        DestroyIfPlatformExists();
        Debug.Log("Deletion completed");
        Debug.Log("Is parent platform null after deletion method: " + GameObject.Find("Platform") == null);

        Vector3 blockSize = prefabs[BlockType.BASICBLOCK].transform.localScale;
        float count = 0;

        List<Gene> genes = chromosone.genes;
        List<Allele> position = genes.Select(x => x.allele).ToList();

        foreach (Allele i in position)
        {
            GameObject block1 = Instantiate(prefabs[BlockType.BASICBLOCK], new Vector3(i.blockPositions[0][0], i.blockPositions[0][1], i.blockPositions[0][2]), orientation);
            block1.transform.parent = GameObject.Find("Platform").transform;

            GameObject block2 = Instantiate(prefabs[BlockType.BASICBLOCK], block1.transform.position + i.blockPositions[1], orientation);
            block2.transform.parent = GameObject.Find("Platform").transform;

            GameObject block3 = Instantiate(prefabs[BlockType.BASICBLOCK], block2.transform.position + i.blockPositions[2], orientation);
            block3.transform.parent = GameObject.Find("Platform").transform;

            count = count + blockSize.x;
        }


        // place object on first brick in lists
        PlacePlayer(position[0].blockPositions[0], orientation);

        GameObject.Find("Platform").transform.position = new Vector3();
        GameObject.Find("Platform").transform.localScale = GameObject.Find("Platform").transform.localScale / 6;
        return GameObject.Find("Platform");
    }

    private void PlacePlayer(Vector3 position, Quaternion rotation)
    {
        // for now move player with platform. Later change this so that platform created first. locked. then player placed
        GameObject player = GameObject.Find("ARPlayer");
        player.transform.position = position + BlockPosition.UP * 10;
        player.transform.parent = GameObject.Find("Platform").transform;
    }

    private void DestroyIfPlatformExists()
    {
        if (placedObject != null)
        {
            Debug.Log("Number of transforms in placedObject: " + placedObject.transform.childCount);
            foreach (Transform child in placedObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            Debug.Log("Number of transforms in placedObject after deletions: " + placedObject.transform.childCount);
        }

    }

    //private void GetCameraAlignment(ARRaycastHit _planeHit, out Quaternion orientation)
    //{
    //    TrackableId planeHit_ID = _planeHit.trackableId;
    //    ARPlane planeHit = _arPlaneManager.GetPlane(planeHit_ID);
    //    Vector3 planeNormal = planeHit.normal;
    //    orientation = Quaternion.FromToRotation(Vector3.up, planeNormal);
    //}
}