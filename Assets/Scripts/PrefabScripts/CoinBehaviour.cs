using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class CoinBehaviour : MonoBehaviour
{
    void FixedUpdate()
    {
        gameObject.transform.Rotate(new Vector3(0, 1, 0), 0.5f);
    }

    void OnCollisionEnter(Collision collision)
    {
        var agent = collision.gameObject.GetComponent<Agent>();
        var player = collision.gameObject.GetComponent<ToadController>();
        if (agent != null)
        {
            agent.AddReward(0.1f);
            Destroy(this.gameObject);
        }
        if (player != null)
        {
            player.numOfCoins++;
            Destroy(this.gameObject);
        }
    }
}
