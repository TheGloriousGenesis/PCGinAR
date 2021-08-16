using System.Collections.Generic;
using Generators;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Implementation;
using GeneticAlgorithms.Parameter;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Utilities;

namespace UI
{
    // inspired by
    // https://github.com/dilmerv/UnityARFoundationEssentials/blob/8a62d21952fad728293a577ee1a13130c5888283/Assets/Scripts/PlacementWithDraggingDroppingController.cs#L51
    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARRaycastManager))]
    [RequireComponent(typeof(ARAnchorManager))]
    [RequireComponent(typeof(ARSessionOrigin))]
    public class PlacementDragAndLock : MonoBehaviour
    {
        #region Game variables
        private GameObject placedObject;
        
        private GameObject placedPrefab;
        public GameObject testSphere;
        
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

        #endregion
        
        #region AR variables

        private ARRaycastManager m_ARRaycastManager;

        private ARPlaneManager m_ARPlaneManager;
        
        private ARAnchorManager m_ARAnchorManager;
        
        private ARSessionOrigin m_ARSessionOrigin;
        
        private Scaler m_scaler;                      // We'll use this to change the reference for scaling

        [SerializeField]
        private Camera arCamera;

        // know what objects we touching
        private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

        private Pose currentPose;

        #endregion

        private List<ARAnchor> anchors = new List<ARAnchor>();
        
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
            m_ARRaycastManager = GetComponent<ARRaycastManager>();
            m_ARPlaneManager = GetComponent<ARPlaneManager>();
            m_ARAnchorManager = GetComponent<ARAnchorManager>();
            m_ARSessionOrigin = GetComponent<ARSessionOrigin>();
            SetAllPlanesActive(false);
#endif
            m_scaler = GetComponent<Scaler>();
            // EnhancedTouchSupport.Enable();
            
            if (lockButton != null)
            {
                lockButton.onClick.AddListener(Lock);
            }

            if (generateButton != null && inGenerationMode == false && isGenerated == false)
            {
                generateButton.onClick.AddListener(Generate);
            }
        }

        // method concerns itself with selection and dragging of prefab
        private void Update()
        {
            if (currentGame == null)
                return;
            if (isLocked)
                return;
#if !UNITY_EDITOR
            if (Input.touchCount == 0)
                return;

            var touch = Input.GetTouch(0);
            
            bool isOverUI = touch.position.IsTouchingUI();

            if (isOverUI) return;
            
            if (touch.phase != TouchPhase.Began)
                return;

            // Raycast against planes and feature points
            const TrackableType trackableTypes =
                TrackableType.FeaturePoint |
                TrackableType.PlaneWithinPolygon;

                // Perform the raycast
            if (m_ARRaycastManager.Raycast(touch.position, hits, trackableTypes))
            {
                // Raycast hits are sorted by distance, so the first one will be the closest hit.
                var hit = hits[0];

                // Create a new anchor
                var anchor = CreateAnchor(hit);
                if (anchor)
                {
                    RemoveAllAnchors();
                    // Remember the anchor so we can remove it later.
                    anchors.Add(anchor);
                }
                else
                {
                    ARDebugManager.Instance.LogInfo("Error creating anchor");
                }
                if (m_scaler != null)
                {
                    m_scaler.referenceToScale.transform.position = hit.pose.position;
                }
                else
                {
                    Debug.Log("Error: Scaler has not being initialized");
                }                
            }
#else
            if (currentGame == null) 
                return;
            if (placedPrefab == null)
                placedPrefab = gameService.CreateGameInPlay(new Vector3(1,2,3), Quaternion.identity,
                    currentGame);
            
            placedObject = placedPrefab;
#endif
        }
        
        #region UI methods

        private void Lock()
        {
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

            gameService.ResetGame(Utility.SafeDestroyInPlayMode);

            EventManager.current.GAStart();
            Chromosome gameData = _gaImplementation.RunGA();
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
            // probs change this
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
        
        ARAnchor CreateAnchor(in ARRaycastHit hit)
        {
            float ActualHeight = Vector3.Distance(hit.pose.position, new Vector3(hit.pose.position.x, 
                arCamera.transform.position.y, hit.pose.position.z));
 
            Debug.Log(ActualHeight);
            if (ActualHeight < 0.8f)
            {
                ARDebugManager.Instance.LogInfo("Plane Low");
            }
            else
            {
                ARDebugManager.Instance.LogInfo("Plane High");
            }
            m_ARSessionOrigin.transform.position = new Vector3(0, -Constants.MAX_PLATFORM_DIMENSION_Y, 0);
            m_ARSessionOrigin.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            ARAnchor anchor = null;
            // // If we hit a plane, try to "attach" the anchor to the plane
            // if (hit.trackable is ARPlane plane)
            // {
            //     var planeManager = GetComponent<ARPlaneManager>();
            //     
            //     if (planeManager)
            //     {
            //         if (currentGame == null) 
            //             return null;
            //         if (placedPrefab == null) {
            //             placedObject = gameService.CreateGameInPlay(hit.pose.position, hit.pose.rotation, 
            //                 currentGame);
            //             placedPrefab = placedObject;
            //         }
            //
            //         ARDebugManager.Instance.LogInfo("Creating anchor attachment.");
            //         
            //         var oldPrefab = m_ARAnchorManager.anchorPrefab;
            //         m_ARAnchorManager.anchorPrefab = placedObject;
            //         anchor = m_ARAnchorManager.AttachAnchor(plane, hit.pose);
            //         m_ARAnchorManager.anchorPrefab = oldPrefab;
            //         
            //         ARDebugManager.Instance.LogInfo($"Anchor {anchor.trackableId}:  Attached to plane {plane.trackableId}");
            //         generateButton.GetComponentInChildren<Text>().text = "End Game";
            //         welcomePanel.SetActive(false);
            //         return anchor;
            //     }
            // }
            //
            // // Otherwise, just create a regular anchor at the hit pose
            // ARDebugManager.Instance.LogInfo("Creating regular anchor.");
            //
            // Note: the anchor can be anywhere in the scene hierarchy
            if (placedPrefab == null)
            {
                placedObject = gameService.CreateGameInPlay(hit.pose.position, hit.pose.rotation, 
                    currentGame);
                placedPrefab = placedObject;
            }

            // var gameObject = placedObject;
            
            // // Make sure the new GameObject has an ARAnchor component
            // anchor = gameObject.GetComponent<ARAnchor>();
            // if (anchor == null)
            // {
            //     anchor = gameObject.AddComponent<ARAnchor>();
            // }
            //
            // ARDebugManager.Instance.LogInfo($"Anchor {anchor.trackableId}: Anchor (from {hit.hitType})");
            return anchor;
        }
        
        public void RemoveAllAnchors()
        {
            ARDebugManager.Instance.LogInfo($"Removing all anchors ({anchors.Count})");
            foreach (var anchor in anchors)
            {
                Destroy(anchor.gameObject);
            }
            anchors.Clear();
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