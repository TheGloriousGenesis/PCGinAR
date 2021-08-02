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

        private Vector2 touchPosition = default;
        
        #endregion
        
        #region AR variables
        
        private ARRaycastManager arRaycastManager;

        [SerializeField]
        private Camera arCamera;

        // know what objects we touching
        private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
        
        [SerializeField]
        private float defaultRotation = 180;
        #endregion

        private float initialDistance;
        
        private Vector3 initialScale;

        private void Awake()
        {
            arRaycastManager = GetComponent<ARRaycastManager>();
            
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
            // if (generateButton.GetComponentInChildren<Text>().text == "Ended")
            // {
            //     gameEnded = true;
            //     
            // }
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

            switch (touch.phase)
            {
                case TouchPhase.Began:
                {
                    // convert screen point to action ray
                    var ray = arCamera.ScreenPointToRay(touch.screenPosition);
                    RaycastHit hitObject;
                    if (Physics.Raycast(ray, out hitObject))
                    {
                        GameObject tmp = hitObject.transform.gameObject;
                        if (tmp != null)
                        {
                            // determines if object can be dragged or nah
                            onTouchHold = !isLocked;
                        }
                    }
                    break;
                }
                case TouchPhase.Ended:
                    onTouchHold = false;
                    break;
            }
            
            MoveGameObject();

            PinchAndZoom();
        }

        private void MoveGameObject()
        {
            if (!arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon)) return;
            var hitPose = hits[0].pose;

            if (placedPrefab == null)
            {
                
                Debug.Log("Please press Generate button to create your level!");
            }
            else
            {
                placedObject = placedPrefab;
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
            gameService.ResetGame(Utility.SafeDestroyInEditMode);

            //todo: check orientation in gameplay to see if this affects it. if not, delete parameter of method
            EventManager.current.GAStart();
            Chromosome gameData = _gaImplementation.Run();
            EventManager.current.GAEnd();
            isGenerated = true;

            placedPrefab = gameService.CreateGameInPlay(new Vector3(), Quaternion.identity, 
               gameData );
            
            //todo: check if this starts the timer properly
            EventManager.current.GameStart();

            generateButton.GetComponentInChildren<Text>().text = isGenerated ? "End Game" : "Generate";
            inGenerationMode = false;
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