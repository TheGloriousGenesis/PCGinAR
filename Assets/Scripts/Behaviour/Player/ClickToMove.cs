using UnityEngine;
using UnityEngine.AI;

// Use physics raycast hit from mouse click to set agent destination
[RequireComponent(typeof(NavMeshAgent))]
public class ClickToMove : MonoBehaviour
{
    NavMeshAgent m_Agent;
    RaycastHit m_HitInfo = new RaycastHit();
    private GameObject target;
    private NavMeshPath path;
    public float m_Range = 25f;

    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        target = GameObject.FindGameObjectWithTag("Goal");
        path = new NavMeshPath();
    }

    void Update()
    {
        if (m_Agent.pathPending || m_Agent.remainingDistance > 0.1f)
            return;

        m_Agent.destination = m_Range * Random.insideUnitCircle;
        
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
