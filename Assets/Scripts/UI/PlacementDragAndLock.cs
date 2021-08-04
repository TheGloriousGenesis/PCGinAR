using System.Collections.Generic;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Implementation;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Utilities;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace UI
{
    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARRaycastManager))]
    public class PlacementDragAndLock : MonoBehaviour
    {
        #region Game variables
        [SerializeField]
        private GameObject placedObject;
        
        private GameObject placedPrefab;
        
        [SerializeField] 
        private GaImplementation _gaImplementation;
        
        [SerializeField] 
        private GenerateGameService gameService;

        private Chromosome currentGame;
        
        #endregion

        #region UI additions
        [SerializeField]
        private GameObject welcomePanel;

        [SerializeField]
        private Button lockButton;

        [SerializeField]
        private Button generateButton;
        
        // [SerializeField]
        // private Button endButton;
        
        private bool isLocked = false;
        private bool isGenerated = false;
        private bool inGenerationMode = false;
        
        // know if we touching screen to drag object around
        private bool onTouchHold = false;

        
        #endregion
        
        #region AR variables
        
        private ARRaycastManager arRaycastManager;

        private ARPlaneManager m_ARPlaneManager;

        [SerializeField]
        private Camera arCamera;

        // know what objects we touching
        private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
        
        [SerializeField]
        private float defaultRotation = 180;
        
        private Vector2 touchPosition = default;

        #endregion

        private float initialDistance;
        
        private Vector3 initialScale;

        private void Awake()
        {
            arRaycastManager = GetComponent<ARRaycastManager>();
            m_ARPlaneManager = GetComponent<ARPlaneManager>();

            EnhancedTouchSupport.Enable();
            
            if (lockButton != null)
            {
                lockButton.onClick.AddListener(Lock);
            }

            if (generateButton != null && inGenerationMode == false)
            {
                generateButton.onClick.AddListener(Generate);
            }
        }

        // method concerns itself with selection and draggin of prefab
        private void Update()
        {
            if (welcomePanel.activeSelf)
                return;

            var activeTouches = Touch.activeTouches;
            Touch touch;
            // if user touches the screen
            if (activeTouches.Count > 0)
            {
                // Obtain first touch point
                touch = activeTouches[0];
            }
            else
            {
                return;
            }

            bool isOverUI = touch.screenPosition.IsTouchingUI();

            if (isOverUI) return;

            if(Input.touchCount > 0)
            {
                touchPosition = touch.screenPosition;

                if(touch.phase == TouchPhase.Began)
                {
                    Ray ray = arCamera.ScreenPointToRay(touch.screenPosition);
                    RaycastHit hitObject;
                    if(Physics.Raycast(ray, out hitObject))
                    {
                        // allows you to start moving object around
                        if(placedObject != null)
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

            PinchAndZoom();
        }

        private void MoveGameObject()
        {
            if (!arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon)) return;
            var hitPose = hits[0].pose;

            if (placedObject == null && currentGame != null)
            {
                placedPrefab = gameService.CreateGameInPlay(hitPose.position, hitPose.rotation, 
                    currentGame );
                
                // hide game object until player touches screen
                placedObject = placedPrefab;

                isGenerated = true;
                // change generate button to end game 
                generateButton.GetComponentInChildren<Text>().text = isGenerated ? "End Game" : "Generate";
            
                //todo: check if this starts the timer properly
                EventManager.current.GameStart();
            } 
            else if (placedObject == null && currentGame == null)
            {
                Debug.Log("Please press Generate button to create your level!");
            }
            else
            {
                Debug.Log($"onTouchHold value: {onTouchHold}");
                if (!onTouchHold) return;
                // moves object around
                placedObject.transform.position = hitPose.position;
                if (defaultRotation == 0)
                {
                    placedObject.transform.rotation = hitPose.rotation;
                }
            }
        }

        private void PinchAndZoom()
        {
            if (placedObject == null && placedPrefab != null)
            {
                placedObject = placedPrefab;
            }
            if (placedPrefab == null)
            {
                Debug.Log("Placed prefab is null (check placed Object) in PNZ method");
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
            isLocked = !isLocked;
            lockButton.GetComponentInChildren<Text>().text = isLocked ? "Locked" : "Unlocked";

            m_ARPlaneManager.enabled = !m_ARPlaneManager.enabled;
            SetAllPlanesActive(!isLocked);
            EventManager.current.GameLocked(isLocked);
        }
        
        void SetAllPlanesActive(bool value)
        {
            foreach (var plane in m_ARPlaneManager.trackables)
                plane.gameObject.SetActive(value);
        }
        
        private void Generate()
        {
            inGenerationMode = true;
            
            if (generateButton.GetComponentInChildren<Text>().text == "End Game")
            {
                EventManager.current.GameEnd();
            }
            welcomePanel.SetActive(false);

            gameService.ResetGame(Utility.SafeDestroyInPlayMode);

            EventManager.current.GAStart();
            Chromosome gameData = _gaImplementation.Run();
            EventManager.current.GAEnd();

            currentGame = gameData;

            inGenerationMode = false;
            
            //todo: REMOVE WHEN PLAYING IN MOBILE
            placedPrefab = gameService.CreateGameInPlay(Vector3.zero, Quaternion.identity, 
                currentGame );
        }

        // private void ShowResults(GameData gameData)
        // {
        //     Debug.Log($"GameCompletedIn:{gameData.timeCompleted}, LeftMove: {gameData.x}, RightMove: {gameData.y}");
        // }

        private void ResetGenerateButton()
        {
            isGenerated = false;
            generateButton.GetComponentInChildren<Text>().text = "Next Level";
        }
        // activated when this script is added to object
        private void OnEnable()
        {
            // subscribe by adding +=
            EventManager.OnGameEnd += ResetGenerateButton;
            // EventManager.OnSendGameStats += ShowResults;
        }
        
        // activated when this script is added to object
        private void OnDisable()
        {
            //unsubscribe by adding -=
            EventManager.OnGameEnd -= ResetGenerateButton;
            // EventManager.OnSendGameStats -= ShowResults;
        }
        
        private void OnDestroy()
        {
            generateButton.onClick.RemoveAllListeners();
            lockButton.onClick.RemoveAllListeners();
        }

        #endregion
    }
}