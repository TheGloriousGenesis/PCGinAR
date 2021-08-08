using UI;
using UnityEngine;

namespace Behaviour.Platform
{
    [RequireComponent(typeof(BoxCollider))]
    public class GoalBehaviour : MonoBehaviour
    {
        // private void OnTriggerEnter(Collider other)
        // {
        //     if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Agent")) return;
        //     EventManager.current.GameEnd();
        // }
    }
}
