using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Implementation;
using PathFinding;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Utilities;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace UI
{
    // inspired by
    // https://github.com/dilmerv/UnityARFoundationEssentials/blob/8a62d21952fad728293a577ee1a13130c5888283/Assets/Scripts/PlacementWithDraggingDroppingController.cs#L51
    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARRaycastManager))]
    [RequireComponent(typeof(ARAnchorManager))]
    public class PlacementDragAndLock : MonoBehaviour
    {
        #region Game variables
        private GameObject placedObject;
        
        private GameObject placedPrefab;
        
        [SerializeField] 
        private GaImplementation _gaImplementation;
        
        [SerializeField] 
        private GenerateGameService gameService;

        private Chromosome currentGame;
        
        #endregion

        #region UI variables
        [SerializeField]
        private GameObject welcomePanel;

        [SerializeField]
        private Button lockButton;

        [SerializeField]
        private Button generateButton;

        private bool isLocked = false;
        private bool isGenerated = false;
        private bool inGenerationMode = false;
        
        // know if we touching screen to drag object around
        private bool onTouchHold = false;
        
        #endregion
        
        #region AR variables

        private ARRaycastManager arRaycastManager;

        private ARPlaneManager m_ARPlaneManager;
        
        private ARAnchorManager m_ARAnchorManager;

        [SerializeField]
        private Camera arCamera;

        // know what objects we touching
        private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
        
        [SerializeField]
        private float defaultRotation = 180;
        
        private Vector2 touchPosition = default;

        private Pose currentPose;

        #endregion

        private float initialDistance;
        
        private Vector3 initialScale;

        private List<ARAnchor> anchors = new List<ARAnchor>();

        private bool initialized = false;
        
        private void Awake()
        {
            AndroidRuntimePermissions.Permission result =
                AndroidRuntimePermissions.RequestPermission("android.permission.ACTIVITY_RECOGNITION");
            if (result == AndroidRuntimePermissions.Permission.Granted)
            {
                ARDebugManager.Instance.LogInfo("We have permission to access the stepcounter");
            }
            else
            {
                Debug.Log("Permission state: " + result); // No permission
                Application.Quit();
            }
            
#if !UNITY_EDITOR
            arRaycastManager = GetComponent<ARRaycastManager>();
            m_ARPlaneManager = GetComponent<ARPlaneManager>();
            m_ARAnchorManager = GetComponent<ARAnchorManager>();

            SetAllPlanesActive(false);
#endif
            EnhancedTouchSupport.Enable();
            
            if (lockButton != null)
            {
                lockButton.onClick.AddListener(Lock);
            }

            if (generateButton != null && inGenerationMode == false && isGenerated == false)
            {
                generateButton.onClick.AddListener(Generate);
            }
        }

        private void Initialized()
        {
            
        }
        

        // method concerns itself with selection and dragging of prefab
        private void Update()
        {
            if (currentGame == null)
                return;

            #if !UNITY_EDITOR
            var activeTouches = Touch.activeTouches;

            if(Input.touchCount > 0)
            {
                Touch touch = Touch.activeTouches[0];
            
                bool isOverUI = touch.screenPosition.IsTouchingUI();

                if (isOverUI) return;

                touchPosition = touch.screenPosition;

                if(touch.phase == TouchPhase.Began)
                {
                    Ray ray = arCamera.ScreenPointToRay(touch.screenPosition);
                    RaycastHit hitObject;
                    if(Physics.Raycast(ray, out hitObject))
                    {
                        Node placement = hitObject.transform.GetComponent<Node>();
                        if(placement != null)
                        {
                            onTouchHold = isLocked ? false : true;
                        }
                    }
                }

                if(touch.phase == TouchPhase.Ended)
                {
                    onTouchHold = false;
                }
            }
            MoveGameObject();
            // PinchAndZoom();
#else
            if(placedObject != null)
            {
                Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
                Vector3 offset =  placedObject.transform.position - Camera.main.ScreenToWorldPoint(
                    new Vector3(Input.mousePosition.x, Input.mousePosition.y,screenPoint.z));
                Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
                Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
                placedObject.transform.position = curPosition;            
            }
            MoveGameObject();
            #endif
        }

        private void MoveGameObject()
        {
#if !UNITY_EDITOR
            if (isLocked) return;
            if (!arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon)) return;
            var hitPose = hits[0].pose;
#else
            var hitPose = new Pose();
            hitPose.position = Vector3.zero;
            hitPose.rotation = Quaternion.identity;

#endif
            currentPose = hitPose;
            
            if (placedObject == null && currentGame != null)
            {
                placedPrefab = gameService.CreateGameInPlay(currentPose.position, currentPose.rotation, 
                    currentGame );
                
                // hide game object until player touches screen
                placedObject = placedPrefab;

                // change generate button to end game set welcome off
                generateButton.GetComponentInChildren<Text>().text = "End Game";
                welcomePanel.SetActive(false);
            } 
            else if (placedObject == null && currentGame == null)
            {
                ARDebugManager.Instance.LogInfo($"Please press Generate button to create your level!");
            }
#if !UNITY_EDITOR
            ARAnchor anchor = m_ARAnchorManager.AddAnchor(hitPose);
            if (anchor == null) 
                ARDebugManager.Instance.LogInfo($"Error creating reference point");
            else 
            {
                anchors.Add(anchor);
            }
            ARDebugManager.Instance.LogInfo($"Number of anchors: {anchors.Count}");

            if (!onTouchHold) return;

            // moves object around
            ARDebugManager.Instance.LogInfo($"Object can be moved around");
            placedObject.transform.position = hitPose.position;
            if (defaultRotation == 0)
            {
                placedObject.transform.rotation = hitPose.rotation;
            }
#endif
        }

        private void PinchAndZoom()
        {
            if (!isGenerated) return;
            if (placedObject == null && placedPrefab != null)
            {
                placedObject = placedPrefab;
            }
            if (placedPrefab == null)
            {
                Debug.Log($"Placed prefab is null (check placed Object) in PNZ method");
                return;
            }
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
        

        #region UI methods

        private void Lock()
        {
            if (placedObject == null) return;
            isLocked = !isLocked;
            lockButton.GetComponentInChildren<Text>().text = isLocked ? "Locked" : "Unlocked";

#if !UNITY_EDITOR
            SetAllPlanesActive(!isLocked);
#endif
            EventManager.current.GameLocked(true);
        }

        private void Generate()
        {
            inGenerationMode = true;
            
            currentGame = null;
            placedObject = null;
            placedPrefab = null;
            isGenerated = false;
            
            if (generateButton.GetComponentInChildren<Text>().text == "End Game")
            {
                EventManager.current.GameEnd();
            }

            gameService.ResetGame(Utility.SafeDestroyInEditMode);

            EventManager.current.GAStart();
            Chromosome gameData = _gaImplementation.Run();
            EventManager.current.GAEnd();

            currentGame = gameData;

            inGenerationMode = false;
            isGenerated = true;

#if !UNITY_EDITOR
            if (!isLocked) {
                SetAllPlanesActive(true);
            }
#endif
            welcomePanel.SetActive(true);
            generateButton.GetComponentInChildren<Text>().text = "Ready?";
            welcomePanel.GetComponent<Text>().text = "Object ready. Please scan area to place level";
        }

        private void SetAllPlanesActive(bool value)
        {
            m_ARPlaneManager.enabled = value;

            foreach (var plane in m_ARPlaneManager.trackables)
                plane.gameObject.SetActive(value);
        }

        private void ResetGenerateButton()
        {
            isGenerated = false;
            generateButton.GetComponentInChildren<Text>().text = "Next Level";
        }
        
        private void OnEnable()
        {
            EventManager.OnGameEnd += ResetGenerateButton;
        }
        
        private void OnDisable()
        {
            EventManager.OnGameEnd -= ResetGenerateButton;
        }
        
        private void OnDestroy()
        {
            generateButton.onClick.RemoveAllListeners();
            lockButton.onClick.RemoveAllListeners();
        }

        #endregion
    }
}