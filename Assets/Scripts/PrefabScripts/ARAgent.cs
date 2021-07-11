using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ARAgent : Agent
{
    private GenerateGame game;

    private Rigidbody rBody;

    private Transform Target;

    private PlayerControls controls;

    public float fallingThreshold = -8f;

    public float forceMultiplier = 10;

    public List<Vector3> destroyedCoins = new List<Vector3>();

    public System.Random rand = new System.Random();

    private bool goalReached = false;

    void Awake()
    {
        rBody = GetComponent<Rigidbody>();
        controls = new PlayerControls();
        controls.Enable();
        if (Target == null)
        {
            Target = GameObject.FindGameObjectWithTag("Goal").transform;
        }
        if (game == null)
        {
            game = GameObject.FindObjectOfType(typeof(GenerateGame)) as GenerateGame;
        }
    }

    public override void OnEpisodeBegin()
    {
        // used to set up an environment for new episode

        // this sets conditions when agent reaches cube --> episode ends (regenerate platform or move goal)
        // if agent rolls off the platform put back on platform (or should we end episode)

        rBody.velocity = Vector3.zero;

        // Destory coins and reset game map so we can update it with new positions
        game.DestoryCoins();
        game.ResetGameMap();

        // randomly find position for player and place. Update gameplacement map on where player is
        List<Vector3> pos = new List<Vector3>(Utility.gamePlacement.Keys);
        int indexPlayer = rand.Next(pos.Count);
        this.transform.position = pos[indexPlayer] + BlockPosition.UP * 2;
        Utility.gamePlacement[pos[indexPlayer]] = BlockType.PLAYER;

        //remove player position and then place goal. Update gameplacement map on where goal is
        pos.RemoveAt(indexPlayer);
        int indexGoal = rand.Next(pos.Count);
        this.Target.position = pos[indexGoal] + BlockPosition.UP * 2;
        Utility.gamePlacement[pos[indexGoal]] = BlockType.GOAL;

        // place coins where player and goal are not
        game.PlaceCoins();
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
            goalReached = true;
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
 