using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject wreakedBreak;

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.tag == "Player")
    //    {
    //        have the probability that the brick will break on collision
    //        if (Random.value < 0.2)
    //        {
    //            Destroy(gameObject);
    //            Instantiate(wreakedBreak, transform.position, transform.rotation);
    //            Destroy(wreakedBreak);
    //        }
    //        else
    //        {
    //            Destroy(gameObject);
    //        }

    //    }
    //}
}
