using UnityEngine;
using UnityEngine.AI;

// Use physics raycast hit from mouse click to set agent destination
[RequireComponent(typeof(NavMeshAgent))]
public class ClickToMove : MonoBehaviour
{
    NavMeshAgent m_Agent;
    RaycastHit m_HitInfo = new RaycastHit();
    private GameObject target;
    private float elapsed = 0.0f;
    private NavMeshPath path;

    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        target = GameObject.FindGameObjectWithTag("Goal");
        path = new NavMeshPath();
        elapsed = 0.0f;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out m_HitInfo))
            {
                m_Agent.destination = m_HitInfo.point;
                Debug.DrawRay(ray.origin, ray.direction, Color.green);
            }
        }
    }
}
