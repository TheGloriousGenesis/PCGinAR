using System.Collections.Generic;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Parameter;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Utilities;
using Utilities.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

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
    private GameObject placedObject;

    [SerializeField]
    private GenerateGame game;
    
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
        EnhancedTouchSupport.Enable();

        arRaycastManager = GetComponent<ARRaycastManager>();

        if (lockButton != null)
        {
            // when some one presses button call method (in brackets)
            lockButton.onClick.AddListener(Lock);
        }

        if (generateButton != null)
        {
            // when some one presses button call method (in brackets)
            generateButton.onClick.AddListener(delegate
            {
                game.CreateGame(Constants.PLAYERTYPE, new Chromosome());
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
        var activeTouches = Touch.activeTouches;

        // if user touches the screen
        if (activeTouches.Count > 0)
        {
            // Obtain first touch point
            var touch = activeTouches[0];

            bool isOverUI = touch.screenPosition.IsPointOverUIObject();

            if (isOverUI) return;

            if (touch.phase == TouchPhase.Began)
            {
                // convert screen ray to action ray
                Ray ray = arCamera.ScreenPointToRay(touch.screenPosition);
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

            MoveGameObject();

            PinchAndZoom();
        }
    }

    private void MoveGameObject()
    {
        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            if (placedObject == null)
            {
                if (defaultRotation > 0)
                {
                    // changes rotation to camera
                    //placedObject = game.PlaceGame(hitPose.position, Quaternion.identity, Constants.playerType);
                    placedObject.transform.Rotate(Vector3.up, defaultRotation);
                }
                else
                {
                    //placedObject = game.PlaceGame(hitPose.position, hitPose.rotation, Constants.playerType);
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

            if (touchZero.phase == UnityEngine.TouchPhase.Ended || touchZero.phase == UnityEngine.TouchPhase.Canceled ||
                touchOne.phase == UnityEngine.TouchPhase.Ended || touchOne.phase == UnityEngine.TouchPhase.Canceled)
            {
                return;
            }

            if (touchZero.phase == UnityEngine.TouchPhase.Began || touchOne.phase == UnityEngine.TouchPhase.Began)
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
}