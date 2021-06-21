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
        string _tag = collision.gameObject.tag;
        if (_tag == "Agent")
        {
            //ARAgent tmp = collision.gameObject.GetComponent<ARAgent>();
            //tmp.AddReward(0.1f);
            Destroy(this.gameObject);
        }
        if (_tag  ==  "Player")
        {
            collision.gameObject.GetComponent<ToadController>().numOfCoins++;
            Destroy(this.gameObject);
        }
    }
}
