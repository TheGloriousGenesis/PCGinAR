using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

#endif

public class TestMeshDistanceCal : MonoBehaviour
{
	public void Start()
	{
		CalculateDistance();
	}
	public void CalculateDistance()
	{
		Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
		Debug.Log($"Number of mesh vertices: {mesh.vertexCount}");
		Vector3[] vertices = mesh.vertices;
		foreach (Vector3 v in vertices)
		{
			RaycastHit hit;
			// Does the ray intersect any objects excluding the player layer
			if (Physics.Raycast(v, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
			{
				Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
				Debug.Log("Did Hit");
			}
			else
			{
				Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
				Debug.Log("Did not Hit");
			}
		}
		
		// for (int i = 0; i < vertices.Length - 1; i++)
		// {
		// 	RaycastHit hit;
		// 	// Does the ray intersect any objects excluding the player layer
		// 	if (Physics.Raycast(vertices[i], transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
		// 	{
		// 		Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
		// 		Debug.Log("Did Hit");
		// 	}
		// 	else
		// 	{
		// 		Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
		// 		Debug.Log("Did not Hit");
		// 	}			
		//
		// 	for (int j = i + 1; i < vertices.Length; i++)
		// 	{
		// 		// GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		// 		// cube.transform.position = vertices[j];
		// 		// Debug.Log(vertices[i]);
		// 		
		// 		if (Vector3.Distance(vertices[i], vertices[j]) <= 3)
		// 		{
		// 			// Debug.Log("Here");
		// 			// Debug.DrawLine(vertices[i], vertices[j], Color.red, 10);
		// 			Vector3 tmp1 = transform.TransformPoint(vertices[i]);
		// 			Vector3 tmp2 = transform.TransformPoint(vertices[j]);
		// 			Debug.DrawLine(tmp1, tmp2, Color.red, 10);
		// 		}
		// 	}
		// }
		// Debug.Log(Vector3.Distance(vertices[1000], vertices[3000]));
		Debug.Log(Vector3.Distance(vertices[5000], vertices[6000]));
		// Debug.DrawLine(vertices[1000], vertices[3000], Color.red, 10);
		Debug.DrawLine(vertices[5000], vertices[6000], Color.green, 10);
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(TestMeshDistanceCal))]
[CanEditMultipleObjects]
public class Path3D_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (GUILayout.Button("Find distances in Mesh"))
		{
			foreach (var targ in targets)
			{
				((TestMeshDistanceCal)targ).CalculateDistance();
			}
		}
	}
}

#endif