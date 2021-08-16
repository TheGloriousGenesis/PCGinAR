using System.Diagnostics;
using GeneticAlgorithms.Entities;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;
using Utilities.DesignPatterns;
using Debug = UnityEngine.Debug;

namespace Behaviour.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class ToadController : Singleton<ToadController>
    {
        // Influenced by https://github.com/whl33886/GravityCar/blob/master/Assets/CarTest/CarController.cs
        // Influenced by https://www.youtube.com/watch?v=k2bpIXzwcWA&list=RDCMUCHM37DnT_QGJT5Zyl4EmqcA&start_radio=1&t=1107s

        #region Game Components
        [SerializeField]
        private Rigidbody _rigidbody;

        [SerializeField]
        private Transform cameraTransform;
        #endregion

        #region Control variables
        
        [SerializeField]
        private float moveSpeed = 1f;
        [SerializeField]
        private float turnSpeed = 3;
        [SerializeField]
        private float jump = 3f;
        [SerializeField]
        private bool isGrounded = true;

        [SerializeField] 
        private float maxSpeed = 10;

        public bool falling = false;
        public float fallingThreshold = -9f;
        // Maximum angle in degrees that should still count as a floor/"underfoot".
        public float maxIncline = 45.0f;
        private PlayerControls controls;

        #endregion
        
        #region InGame metrics
        
        private int numberOfJumps = 0;
        private int numberOfJoystickMovements = 0;
        private bool goalReached = false;
        private GameData _gameData = new GameData();
        private Stopwatch timer = new Stopwatch();
        
        #endregion

        #region AR Metrics

        private static int INACTIVE_SAMPLE = 12;
        private int currentSample = 0;
        private int stepCount = 0;
        private bool isActiveCounter = true;
        private float accelerometerUpdateInterval = 1.0f / 60.0f;
        private float lowPassKernelWidthInSeconds = 1.0f;
        private float lowPassFilterFactor;
        private Vector3 lowPassValue = Vector3.zero;
        Accelerometer accelerometer;
        
        #endregion
        
        private void Awake()
        {
            controls = new PlayerControls();
            
            accelerometer = Accelerometer.current;

            if (accelerometer != null)
            {
                InputSystem.EnableDevice(accelerometer); 
                lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
                lowPassValue = accelerometer.acceleration.ReadValue();
            }
        }

        private void Start()
        {
            EventManager.current.GameStart();
            timer.Start();
        }

        private void FixedUpdate()
        {
            _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, maxSpeed);
            Move(controls.Player.Move.ReadValue<Vector2>());
            Jump(controls.Player.Jump.ReadValue<float>());
        }

        public void Update()
        {
            if (this.gameObject.activeSelf)
            {
                CheckIfPlayerHasFallen();
            }

            if (accelerometer != null)
            {
                lowPassValue = LowPassFilterAccelerometer(accelerometer.acceleration.ReadValue());
                Detect(Vector3.SqrMagnitude(lowPassValue), 1.3d);
            }
        }

        private void OnEnable()
        {
            controls.Enable();
            EventManager.OnCurrentChromosomeInPlay += ObtainChromosomeIdOfCurrentLevel;
        }

        private void OnDisable()
        {
            controls.Disable();
            EventManager.OnCurrentChromosomeInPlay -= ObtainChromosomeIdOfCurrentLevel;
        }
        
        private void OnCollisionStay(Collision collisionInfo)
        {
            int onground = 0;
            int inair = 0;
            foreach (ContactPoint contact in collisionInfo.contacts)
            {
                _ = IsContactUnderneath(contact) ? onground = onground + 1 : inair = inair + 1;
            }

            if (onground >= inair)
            {
                isGrounded = true;
            } else
            {
                isGrounded = false;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Goal")) return;
            goalReached = true;
            EventManager.current.GameEnd();
        }

        private void OnDestroy()
        {
            EventManager.current.GameEnd();
            if (accelerometer != null)
            {
                InputSystem.DisableDevice(accelerometer);
            }
            timer.Stop();
            _gameData.numberOfInGameMovements = numberOfJoystickMovements;
            _gameData.numberOfJumps = numberOfJumps;
            _gameData.timeCompleted = timer.ElapsedMilliseconds;
            _gameData.goalReached = goalReached;
            // #if !UNITY_EDITOR
            _gameData.numberOfPhysicalMovement = stepCount;
            // #else
            // _gameData.numberOfPhysicalMovement = (int) SimpleRNG.GetNormal(400, 60);
            // #endif
            EventManager.current.SendGameStats(_gameData);
        }
        
        public void Move(Vector2 input)
        {
            Vector3 dir = Vector3.zero;
            dir.x = Mathf.Clamp(input.x, -1f, 1f);
            dir.z = Mathf.Clamp(input.y, -1f, 1f);

            Vector3 camDirection = Camera.main.transform.rotation * dir; //This takes all 3 axes (good for something flying in 3d space)    
            Vector3 targetDirection = new Vector3(camDirection.x, 0, camDirection.z); //This line removes the "space ship" 3D flying effect. We take the cam direction but remove the y axis value

            if (dir != Vector3.zero)
            {
                numberOfJoystickMovements++;
                //turn the character to face the direction of travel when there is input
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(targetDirection),
                    Time.deltaTime * turnSpeed
                );
            }
            
            //normalized prevents char moving faster than it should with diagonal input
            _rigidbody.AddForce(targetDirection.normalized * moveSpeed);
        }
    
        public void Jump(float jumpingValue)
        {
            if (isGrounded && jumpingValue > 0)
            {
                numberOfJumps++;
                _rigidbody.AddForce(new Vector3(0, 2 * jump, 0), ForceMode.Impulse);
                isGrounded = false;
            }
        }

        private bool Detect(double accelerometerValue,  double currentThreshold) {
            
            if (currentSample == INACTIVE_SAMPLE) {
                currentSample = 0;
                if (!isActiveCounter)
                    isActiveCounter = true;
            }
            if (isActiveCounter && (accelerometerValue > currentThreshold)) {
                currentSample = 0;
                isActiveCounter = false;
                Debug.Log($"StepCounter, detect() true for threshold {currentThreshold}");
                stepCount++;
                return true;
            }

            ++currentSample;
            ARDebugManager.Instance.LogInfo($"Current Steps = {stepCount}");
            return false;
        }

        private void ObtainChromosomeIdOfCurrentLevel(int chromosomeId)
        {
            _gameData.chromosomeID = chromosomeId;
        }
        
        private int GETStepCount() {
            return stepCount;
        }
        
        private bool IsContactUnderneath(ContactPoint contact)
        {
            // Ignore collisions with ceilings/walls
            if (contact.normal.y <= 0f)
                return false;

            // Take the contact point into our local space.
            Vector3 local = transform.InverseTransformPoint(contact.point);

            // Flatten & ignore the vertical component.
            // We'll just look at the footprint.
            local.y = 0.0f;

            // Cache this to a variable somewhere 
            // to avoid repeating the trig for every check.
            float threshold = 0.5f * Mathf.Sin(maxIncline * Mathf.Deg2Rad);
            threshold *= threshold;

            return local.sqrMagnitude <= threshold;
        }

        private void CheckIfPlayerHasFallen()
        {
            if (_rigidbody.velocity.y  < fallingThreshold)
            {
                falling = true;
            } else
            {
                falling = false;
            }

            if (falling)
            {
                // numOfCoins -= 100;
                // text.text = "Coin counter: " + numOfCoins.ToString();
                EventManager.current.GameEnd();
                Utility.SafeDestroyInPlayMode(this.gameObject);
            }
        }

        private Vector3 LowPassFilterAccelerometer(Vector3 prevValue)
        {
            Vector3 newValue = Vector3.Lerp(prevValue, accelerometer.acceleration.ReadValue(),
                lowPassFilterFactor);
            return newValue;
        }

    }
}

