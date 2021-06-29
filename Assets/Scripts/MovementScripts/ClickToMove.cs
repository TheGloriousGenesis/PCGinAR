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

        elapsed += Time.deltaTime;

        if (elapsed > 1.0f)
        {
            elapsed -= 1.0f;
            bool pathFound = NavMesh.CalculatePath(transform.position, target.transform.position, NavMesh.AllAreas, path);
            Debug.Log($"is path found: {pathFound}, status: {path.status}");
        }

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Debug.DrawLine(path.corners[i] + Vector3.up, path.corners[i + 1] + Vector3.up, Color.black);
        } 

        //if (path != null)
        //{
        //    m_Agent.SetPath(path);
        //}
    }
}
