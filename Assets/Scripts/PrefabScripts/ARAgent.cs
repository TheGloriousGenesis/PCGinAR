using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ARAgent : Agent
{
    private Rigidbody rBody;

    private Transform Target;

    private PlayerControls controls;

    public float fallingThreshold = -8f;

    public float forceMultiplier = 10;

    public List<Vector3> destroyedCoins = new List<Vector3>();

    void Awake()
    {
        rBody = GetComponent<Rigidbody>();
        controls = new PlayerControls();
        controls.Enable();
        if (Target == null)
        {
            Target = GameObject.FindGameObjectWithTag("Goal").transform;
        }
    }

    public override void OnEpisodeBegin()
    {
        // used to set up an environment for new episode
        // usually initialised in a random manner so agent can solve task in variety of conditions

        // this sets conditions when agent reaches cube --> episode ends (regenerate platform or move goal)
        // if agent rolls off the platform put back on platform (or should we end episode)

        rBody.velocity = Vector3.zero;
        this.transform.position = Utility.currentAgentPosition + Vector3.up * 2;

        destroyedCoins = new List<Vector3>();
        GameObject[] allCoins = GameObject.FindGameObjectsWithTag("Coin");
        if (allCoins != null)
        {
            for (int i = allCoins.Length - 1; i >= 0; i--)
            {
                Utility.SafeDestory(allCoins[i].gameObject);
            }
        }

        CoinGenerator coins = GameObject.FindObjectOfType(typeof(CoinGenerator)) as CoinGenerator;
        coins.PlaceCoins();

        // place target farther than player
        // Move the target to a new spot - should have tiles in array from environment
        //Target.position = Utility.walkableSurface[0] + Vector3.up * 3;
        //Utility.walkableSurface.Remove(Utility.walkableSurface[0]);
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

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.position, Target.position);

        //Debug.Log($"distnace to target: {distanceToTarget}");

        // Fell off platform
        if (rBody.velocity.y < fallingThreshold)
        {
            AddReward(-10.0f);
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Goal")
        {
            Debug.Log("Here in collisions");
            AddReward(10.0f);
            Debug.Log($"Episode {CompletedEpisodes}, Reward: {GetCumulativeReward()}");
            EndEpisode();
        }
    }

    public void AddDestoryedCoinToList(Vector3 coins)
    {
        destroyedCoins.Add(coins);
    }
}
 