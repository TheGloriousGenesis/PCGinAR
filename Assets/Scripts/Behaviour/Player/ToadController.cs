using GeneticAlgorithms.Entities;
using UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Utilities;
using Utilities.DesignPatterns;

namespace Behaviour.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class ToadController : Singleton<ToadController>
    {
        // Influenced by https://github.com/whl33886/GravityCar/blob/master/Assets/CarTest/CarController.cs
        // Influenced by https://www.youtube.com/watch?v=k2bpIXzwcWA&list=RDCMUCHM37DnT_QGJT5Zyl4EmqcA&start_radio=1&t=1107s

        #region Game Components
        [SerializeField]
        private Rigidbody _rigidbody;

        [SerializeField]
        private NavMeshAgent agent;

        [SerializeField]
        private Transform cameraTransform;
        #endregion

        #region Player control variables
        
        private PlayerControls controls;

        [SerializeField]
        private float moveSpeed = 1f;

        [SerializeField]
        private float turnSpeed = 3;

        [SerializeField]
        private float jump = 3f;

        [SerializeField]
        private bool isGrounded;

        public bool falling = false;

        public float fallingThreshold = -9f;

        // Maximum angle in degrees that should still count as a floor/"underfoot".
        public float maxIncline = 45.0f;

        // public bool edgeDetected = false;

        #endregion

        #region UI variables
        private Text playerStats;
        #endregion
        
        public int numOfCoins = 0;
        
        #region Inbuilt Unity Methods
        void Awake()
        {
            controls = new PlayerControls();
        }

        // void Start()
        // {
        //     if (Gyroscope.current != null)
        //     {
        //         InputSystem.EnableDevice(Gyroscope.current);
        //     }
        //     if (AttitudeSensor.current != null)
        //     {
        //         InputSystem.EnableDevice(AttitudeSensor.current);
        //     }
        //
        // }

        public void FixedUpdate()
        {
            //todo: check if this is called when angle of phone moved
            // UnityEngine.InputSystem.Gyroscope.current.angularVelocity.ReadValue();
            // AttitudeSensor.current.attitude.ReadValue();
            
            // edgeDetected = Utility.EdgesOfCurrentGame.Contains(transform.position);

            Move(controls.Player.Move.ReadValue<Vector2>());
            Jump(controls.Player.Jump.ReadValue<float>());
        }

        public void Update()
        {
            if (this.gameObject.activeSelf)
            {
                CheckIfPlayerHasFallen();
            }
            // text.text = "Coin counter: " + numOfCoins.ToString();
            
            // NavMeshHit hit;

            // var edgeHere = NavMesh.FindClosestEdge(transform.position, out hit,
            //     NavMesh.AllAreas);
            //
            // edgeDetected = edgeHere;
            //
            // var sampleResult = NavMesh.SamplePosition(transform.position, out hit, agent.radius,
            //     NavMesh.AllAreas);
            //
            // if (sampleResult && isGrounded)
            // {
            //     transform.position = hit.position;
            //     _rigidbody.isKinematic = true;
            //     agent.enabled = true;
            // }
            // else
            // {
            //     Debug.Log("navmesh agent landed outside navmesh, cannot get back in");
            // }
        }

        private void OnEnable()
        {
            controls.Enable();
        }

        private void OnDisable()
        {
            controls.Disable();
        }
        
        #endregion
        
        #region Player environment check methods
        void OnCollisionStay(Collision collisionInfo)
        {
            int onground = 0;
            int inair = 0;
            foreach (ContactPoint contact in collisionInfo.contacts)
            {
                //Debug.DrawRay(contact.point, contact.normal, Color.white, 2);
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

        bool IsContactUnderneath(ContactPoint contact)
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

        public void CheckIfPlayerHasFallen()
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
                numOfCoins -= 100;
                // text.text = "Coin counter: " + numOfCoins.ToString();
                EventManager.current.GameEnd();
                Utility.SafeDestroyInPlayMode(this.gameObject);
            }
        }

        #endregion
        
        #region Player movement methods
        // Manage the navmesh movement here
        public void Move(Vector2 input)
        {
            Vector3 dir = Vector3.zero;
            dir.x = Mathf.Clamp(input.x, -1f, 1f);
            dir.z = Mathf.Clamp(input.y, -1f, 1f);

            Vector3 camDirection = Camera.main.transform.rotation * dir; //This takes all 3 axes (good for something flying in 3d space)    
            Vector3 targetDirection = new Vector3(camDirection.x, 0, camDirection.z); //This line removes the "space ship" 3D flying effect. We take the cam direction but remove the y axis value

            if (dir != Vector3.zero)
            { 
                EventManager.current.UpdateGameStats(GameDataEnum.X, 1);
                EventManager.current.UpdateGameStats(GameDataEnum.Y, 1);
                //turn the character to face the direction of travel when there is input
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(targetDirection),
                    Time.deltaTime * turnSpeed
                );
            }
            
            // if (edgeDetected)
            // {
            //     Debug.Log("Edge detected");
            //     agent.enabled = false;
            //     _rigidbody.isKinematic = false;
            //     _rigidbody.AddForce(targetDirection.normalized * moveSpeed);
            // }
            agent.enabled = false;
            _rigidbody.isKinematic = false;
            //normalized prevents char moving faster than it should with diagonal input
            _rigidbody.AddForce(targetDirection.normalized * moveSpeed);
        }
    
        // Manage the navmesh movement here
        public void Jump(float jumpingValue)
        {
            if (isGrounded && jumpingValue > 0)
            {
                EventManager.current.UpdateGameStats(GameDataEnum.JUMP, 1);
                isGrounded = false;
                
                agent.enabled = false;
                _rigidbody.isKinematic = false;
                
                _rigidbody.AddForce(new Vector3(0, 2 * jump, 0), ForceMode.Impulse);
            }
        }

        #endregion
    }
}

