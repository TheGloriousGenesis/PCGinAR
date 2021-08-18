using System.Collections.Generic;
using System.Diagnostics;
using Behaviour.Entities;
using Generators;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Implementation;
using GeneticAlgorithms.Parameter;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Utilities;
using Debug = UnityEngine.Debug;

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
        
        protected Stopwatch timer = new Stopwatch();
        
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
                CreateAnchor(hit);

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
                placedPrefab = gameService.CreateGame(new Vector3(1,2,3), Quaternion.identity,
                    currentGame, BlockType.PLAYER);
            
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
            timer.Reset();
            timer.Start();
            Chromosome gameData = _gaImplementation.RunGA();
            timer.Stop();
            Debug.Log($"Run method takes : {timer.ElapsedMilliseconds}");
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
        
        void CreateAnchor(in ARRaycastHit hit)
        {
            // if (hit.trackable is ARPlane plane)
            // {
            //     var transform1 = hit.trackable.transform;
            //     var position = transform1.position;
            //     Debug.Log($"plane position: {position}, " +
            //               $"Area: {position.x * position.y}");
            // }
            // Note: the anchor can be anywhere in the scene hierarchy
            if (placedPrefab == null && !isLocked)
            {
                placedPrefab = gameService.CreateGame(hit.pose.position, hit.pose.rotation, 
                    currentGame, BlockType.PLAYER);
                placedObject = placedPrefab;
                generateButton.GetComponentInChildren<Text>().text = "End Game";
                Debug.Log("Placed prefab is created");
            }
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