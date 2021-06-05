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
    private Button lockButton;

    [SerializeField]
    private Button generateButton;

    [SerializeField]
    private Camera arCamera;

    [SerializeField]
    private float defaultRotation = 180;

    [SerializeField]
    private GameObject player;

    private GameObject placedObject;

    private Vector2 touchPosition = default;

    private ARRaycastManager arRaycastManager;

    private bool isLocked = false;

    // know if we touching screen
    private bool onTouchHold = false;

    // know what objects we touching
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private BasicGeneticAlgorithm bga = new BasicGeneticAlgorithm();

    public PrefabFactory prefabs;

    public BlockTile[] blockTiles;

    private float initialDistance;

    private Vector3 initialScale;

    private Chromosone currentSolution;

    private List<Vector3> walkableSurface;

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
            generateButton.onClick.AddListener(delegate {
                CreateGame(new Vector3(), Quaternion.identity);
            });
        }
    }

    private void OnDestroy()
    {
        generateButton.onClick.RemoveAllListeners();
        lockButton.onClick.RemoveAllListeners();
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
                    if (placedObject != null)
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
                    placedObject = CreateGame(hitPose.position, Quaternion.identity);
                    placedObject.transform.Rotate(Vector3.up, defaultRotation);
                }
                else
                {
                    placedObject = CreateGame(hitPose.position, hitPose.rotation);
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

    private GameObject CreateGame(Vector3 plane, Quaternion orientation)
    {
        ResetGameArea();
        GameObject platform = PlacePlatform(orientation);
        walkableSurface = ObtainWalkableSurface(platform).ToList();
        player = PlacePlayer(walkableSurface[0], orientation);
        PlaceGoal();
        PlaceCoins();
        return ConfigureGameSpace(plane);
    }

    private GameObject ConfigureGameSpace(Vector3 plane)
    {
        GameObject game = GameObject.Find("/GAME");

        // This line might not be needed. Why dont i try placing object in front of camera using camera transformation.
        game.transform.position = plane;

        // Might be able to set platform scale before hand. Maybe do a generic config file that sets scales and rotation for each asset attached?
        // have tried to rescale before brick added and that didnt work so think about it
        //GameObject.Find("/GAME").transform.localScale = GameObject.Find("/GAME").transform.localScale;
        return game;
    }
    // this might be compute in the fitness helper genetic algorithm. Also think about normalising with respect to plane touched for AR

    private GameObject PlacePlatform(Quaternion orientation)
    {
        Chromosone chromosone = bga.GenerateChromosome(4);
        currentSolution = chromosone;

        GameObject platform = GameObject.Find("/GAME/Platform");

        Vector3 blockSize = prefabs[BlockType.BASICBLOCK].transform.localScale;

        List<Gene> genes = chromosone.genes;
        List<Allele> position = genes.Select(x => x.allele).ToList();

        foreach (Allele i in position)
        {
            GameObject block1 = Instantiate(prefabs[BlockType.BASICBLOCK], new Vector3(i.blockPositions[0][0], i.blockPositions[0][1], i.blockPositions[0][2]), orientation);
            block1.transform.parent = platform.transform;

            GameObject block2 = Instantiate(prefabs[BlockType.BASICBLOCK], block1.transform.position + i.blockPositions[1], orientation);
            block2.transform.parent = platform.transform;

            GameObject block3 = Instantiate(prefabs[BlockType.BASICBLOCK], block2.transform.position + i.blockPositions[2], orientation);
            block3.transform.parent = platform.transform;
        }

        return platform;
    }

    private GameObject PlacePlayer(Vector3 position, Quaternion rotation)
    {
        player.SetActive(true);
        player.transform.position = position + BlockPosition.UP * 10;
        player.transform.localScale = new Vector3(1, 1, 1);
        //remove this tile from walkable surface
        walkableSurface.Remove(position);
        return player;
    }

    // bricks can be placed on top of each other so much find a way to get a list of all top bricks
    private void PlaceGoal()
    {
        float maxDistance = 0f;
        Vector3 farthestBrick = new Vector3();

        foreach (Vector3 child in walkableSurface)
        {
            float currentDistance = Vector3.Distance(player.transform.position, child);

            //Debug.Log("Current Distance: " + currentDistance);
            if (currentDistance > maxDistance)
            {
                maxDistance = currentDistance;
                farthestBrick = child;
                
                //Debug.Log("Distance is: " + currentDistance);
            }
        }
        
        GameObject goal_ = Instantiate(prefabs[BlockType.GOAL], farthestBrick + Vector3.up * 2, Quaternion.identity);
        goal_.transform.parent = GameObject.Find("/GAME").transform;
        //remove this tile from walkable surface
        walkableSurface.Remove(farthestBrick);
    }

    private void PlaceCoins()
    {
        foreach(Vector3 i in walkableSurface)
        {
            GameObject coin = Instantiate(prefabs[BlockType.COIN], i + Vector3.up * 2, Quaternion.identity);
            coin.transform.parent = GameObject.Find("/GAME/Coins").transform;
        }
    }
    
    private void ResetGameArea()
    {
        GameObject[] platform = GameObject.FindGameObjectsWithTag("Brick");
        if (platform != null)
        {
            for (int i = platform.Length - 1; i>=0; i--)
            {
                SafeDestory(platform[i].gameObject);
            }
        }

        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        if (coins != null)
        {
            for (int i = coins.Length - 1; i >= 0; i--)
            {
                SafeDestory(coins[i].gameObject);
            }
        }

        GameObject[] goal = GameObject.FindGameObjectsWithTag("Goal");
        if (goal != null)
        {
            for (int i = goal.Length - 1; i >= 0; i--)
            {
                SafeDestory(goal[i].gameObject);
            }
        }

        // Instead of deleting reset position of pole and player at origin
        if (player != null)
        {
            player.transform.position = Vector3.up * 10;
            player.SetActive(false);
        }

    }

    // Inspired from https://forum.unity.com/threads/so-why-is-destroyimmediate-not-recommended.526939/
    public static void SafeDestory(GameObject obj)
    {
        obj.transform.parent = null;
        obj.name = "$disposed";
        UnityEngine.Object.Destroy(obj);
        obj.SetActive(false);
    }

    private HashSet<Vector3> ObtainWalkableSurface(GameObject platform)
    {
        HashSet<Vector3> surface = new HashSet<Vector3>();
        foreach (Transform child in platform.transform)
        {
            RaycastHit hit;

            if (!Physics.Raycast(child.transform.position, Vector3.up * 1.5f, out hit))
            {
                //Debug.DrawRay(child.transform.position, Vector3.up * 10, Color.green, 25);
                surface.Add(child.transform.position);
            } else
            {
                //Debug.Log("Child: " + child.transform.position + " Hit: " + hit.transform.gameObject.name + " at position: " + hit.transform.position);
            }
        }
        return surface;
    }

    //private void GetCameraAlignment(ARRaycastHit _planeHit, out Quaternion orientation)
    //{
    //    TrackableId planeHit_ID = _planeHit.trackableId;
    //    ARPlane planeHit = _arPlaneManager.GetPlane(planeHit_ID);
    //    Vector3 planeNormal = planeHit.normal;
    //    orientation = Quaternion.FromToRotation(Vector3.up, planeNormal);
    //}
}