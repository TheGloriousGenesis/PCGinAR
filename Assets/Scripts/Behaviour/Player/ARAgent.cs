using System.Collections.Generic;
using Behaviour.Entities;
using GeneticAlgorithms.Parameter;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Utilities;
using Random = System.Random;

namespace Behaviour.Player
{
    // player can follow A* path and detect number of turns etc jumps
    public class ARAgent : Agent
    {
        private GenerateGameService _gameService;

        private Rigidbody rBody;

        private Transform Target;

        private ToadController ARPlayer;

        public float fallingThreshold = -8f;

        public PlayerControls controls;
        
        public Random rand = new Random(Constants.SEED);

        private bool _goalReached = false;

        void Awake()
        {
            ARPlayer = GetComponent<ToadController>();
            rBody = GetComponent<Rigidbody>();
            
            if (Target == null)
            {
                Target = GameObject.FindGameObjectWithTag("Goal").transform;
            }
            if (_gameService == null)
            {
                _gameService = FindObjectOfType(typeof(GenerateGameService)) as GenerateGameService;
            }
        }

        public override void OnEpisodeBegin()
        {
            // used to set up an environment for new episode

            // this sets conditions when agent reaches cube --> episode ends (regenerate platform or move goal)
            // if agent rolls off the platform put back on platform (or should we end episode)

            rBody.velocity = Vector3.zero;
            
            // think about updating map and positions of the player goal so maybe can place coins
            Vector3 posPlayer = Utility.GetKRandomElements(Utility.GetGameMap()[BlockType.FREE_TO_WALK], 1, 
                rand)[0];
            this.transform.position = posPlayer + BlockPosition.UP * 2;
            
            Vector3 posGoal = Utility.GetKRandomElements(Utility.GetGameMap()[BlockType.FREE_TO_WALK], 1, 
                rand)[0];
            this.Target.position = posGoal + BlockPosition.UP * 2;
        }

        // information to collect to send to brain to make decision
        public override void CollectObservations(VectorSensor sensor)
        {
            // 6 floats here for position
            sensor.AddObservation(Target.localPosition);
            sensor.AddObservation(this.transform.localPosition);

            // 3 floats here for velocity
            sensor.AddObservation(rBody.velocity.x);
            sensor.AddObservation(rBody.velocity.z);
            sensor.AddObservation(rBody.velocity.y);
        }

        // receives actions and assigns rewards
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            // Obtain actions
            var actions = actionBuffers.ContinuousActions;

            Vector2 controlSignal = Vector2.zero;

            controlSignal.x = actions[0];
            controlSignal.y = actions[2];

            ToadController.Instance.Move(controlSignal);
            ToadController.Instance.Jump(actions[1]);
        }

        // Overwrites input actions with keyboard input
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;

            continuousActionsOut[0] = controls.Player.Move.ReadValue<Vector2>()[0];

            continuousActionsOut[1] = controls.Player.Jump.ReadValue<float>() > 0 ? 1 : 0;

            continuousActionsOut[2] = controls.Player.Move.ReadValue<Vector2>()[1];
        }

        private void FixedUpdate()
        {
            // Fell off platform
            if (rBody.velocity.y < fallingThreshold)
            {
                AddReward(-10.0f);
                EndEpisode();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Goal")) return;
            _goalReached = true;
            AddReward(10.0f);
            Debug.Log($"Episode {CompletedEpisodes}, Reward: {GetCumulativeReward()}");
            EndEpisode();
        }

    }
}
 