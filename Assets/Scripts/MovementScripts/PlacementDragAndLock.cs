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

    private GameObject placedObject;

    [SerializeField]
    private Text playerStats;

    private Vector2 touchPosition = default;

    private ARRaycastManager arRaycastManager;

    private bool isLocked = false;

    // know if we touching screen
    private bool onTouchHold = false;

    // know what objects we touching
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
  

    private float initialDistance;

    private Vector3 initialScale;

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
        playerStats.text = "Coin counter: 0";
        ResetGameArea();

        PlatformGenerator platform = GameObject.FindObjectOfType(typeof(PlatformGenerator)) as PlatformGenerator;
        platform.CreatePlatform(plane, orientation);

        CoinGenerator coins = GameObject.FindObjectOfType(typeof(CoinGenerator)) as CoinGenerator;
        coins.PlaceCoins();

        return ConfigureGameSpace(plane);
    }

    private GameObject ConfigureGameSpace(Vector3 plane)
    {
        GameObject game = GameObject.Find("/GAME");

        // This line might not be needed. Why dont i try placing object in front of camera using camera transformation.
        game.transform.position = plane;
        game.transform.rotation = Quaternion.Inverse(game.transform.rotation);

        // Might be able to set platform scale before hand. Maybe do a generic config file that sets scales and rotation for each asset attached?
        // have tried to rescale before brick added and that didnt work so think about it
        //GameObject.Find("/GAME").transform.localScale = GameObject.Find("/GAME").transform.localScale;
        return game;
    }

    private void ResetGameArea()
    {
        GameObject[] platform = GameObject.FindGameObjectsWithTag("Brick");
        if (platform != null)
        {
            for (int i = platform.Length - 1; i>=0; i--)
            {
                Utility.SafeDestory(platform[i].gameObject);
            }
        }

        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        if (coins != null)
        {
            for (int i = coins.Length - 1; i >= 0; i--)
            {
                Utility.SafeDestory(coins[i].gameObject);
            }
        }

        GameObject[] goal = GameObject.FindGameObjectsWithTag("Goal");
        if (goal != null)
        {
            for (int i = goal.Length - 1; i >= 0; i--)
            {
                Utility.SafeDestory(goal[i].gameObject);
            }
        }

        GameObject[] player = GameObject.FindGameObjectsWithTag("Player");
        if (player != null)
        {
            for (int i = player.Length - 1; i >= 0; i--)
            {
                Utility.SafeDestory(player[i].gameObject);
            }
        }

    }
}