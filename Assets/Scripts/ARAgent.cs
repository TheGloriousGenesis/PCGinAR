using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ARAgent : Agent
{
    private Rigidbody rBody;

    public Transform Target;

    private PlayerControls controls;

    public float fallingThreshold = -8f;

    public List<Vector3> walkableSurface;

    public float forceMultiplier = 10;

    //private ToadController toadController;

    void Awake()
    {
        rBody = GetComponent<Rigidbody>();
        controls = new PlayerControls();
        controls.Enable();
        // should I also obtain platform in this method?
        walkableSurface = new List<Vector3>();
        //toadController = GetComponent<ToadController>();
    }

    public override void OnEpisodeBegin()
    {
        // used to set up an environment for new episode
        // usually initialised in a random manner so agent can solve task in variety of conditions

        // this sets conditions when agent reaches cube --> episode ends (regenerate platform or move goal)
        // if agent rolls off the platform put back on platform (or should we end episode)


        // should generate platform


        // if agent is falling
        if (rBody.velocity.y < fallingThreshold)
        {
            this.transform.localPosition = Utility.currentAgentPosition;
        }

        // place target farther than player
        // Move the target to a new spot - should have tiles in array from environment
        Target.localPosition = Utility.walkableSurface[0];
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

    // recieves actions and assigns rewards
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Obtain actions
        var actions = actionBuffers.ContinuousActions;

        Vector3 controlSignal = Vector3.zero;

        // Clamp to remove extreme values
        for (var i = 0; i < actionBuffers.ContinuousActions.Length; i++)
        {
            actions[i] = Mathf.Clamp(actions[i], -1f, 1f);
        }

        controlSignal.x = actions[0];
        controlSignal.y = 0;
        controlSignal.z = actions[2];

        //var jump = ScaleAction(actions[1], 0, 1);

        // smooth walking add to toad controller
        rBody.AddForce((controlSignal) * forceMultiplier);
        rBody.AddForce(new Vector3(0, actions[1] * 0.5f, 0), ForceMode.Impulse);

        //orientation = controlSignal;

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // Maybe also set reward when coin is obtained? 

        // Fell off platform
        else if (rBody.velocity.y < fallingThreshold)
        {
            EndEpisode();
        }
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
        
    }

    private void Update()
    {
        
    }
}
 