using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

#endif
public class GraphPrintAllPaths {

	public void print(Node start, Node end, string path, Dictionary<Node, Boolean> visited){
        String newPath = path +  "->" + start.positionData;
        visited[start] = true;
        List<Node> list = start.neighbours;
        for (int i = 0; i < list.Count ; i++) {
            Node node = list[i];
            if(node!=end && visited[node]==false){
                print(node,end,newPath,visited);
            }else if(node==end){
                Debug.Log(newPath + "->" + node.positionData);
            }
        }
        //remove from path
        visited[start] = false;
    }

	public void printAllPaths(List<Node> graph, Node start, Node end){
		Dictionary<Node, Boolean> visited = graph.ToDictionary(x => x, x => false);
        visited[start] = true;
        print(start, end, "", visited);
    }
}

public class Path3D: MonoBehaviour {

	#region graph variables
	private Dictionary<Vector3, GameObject> positionMap = new Dictionary<Vector3, GameObject>();
	private Dictionary<Vector3, Node> nodeMap = new Dictionary<Vector3, Node>();
	private List<Node> allNodesInScene = new List<Node>();
	#endregion

	[SerializeField]
	private GameObject platform;
	
	public void StartThisMethod() {
		Debug.Log("Starting Pathfinding..");
		FindAllNodes();
		Debug.Log("Nodes in scene found..");
		CreateAdjacencyMatrix();
		Debug.Log("Adjacency Matrix created..");
		GraphPrintAllPaths p = new GraphPrintAllPaths();
		Node player = allNodesInScene[allNodesInScene.Count - 1];
		Node target = allNodesInScene[0];
		Debug.Log($"Start Node position..{player.positionData}");
		Debug.Log($"End Node position..{target.positionData}");
		System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
		timer.Start();
		p.printAllPaths(allNodesInScene,player,target);
		timer.Stop();
		Debug.Log($"Time elapsed: {timer.Elapsed.TotalMilliseconds}");
	}
	
	public void FindAllNodes() {
		foreach (Transform child in platform.transform)
		{
			Vector3 position = child.position;
			GameObject o;
			Node tmp = (o = child.gameObject).GetComponent<Node>();
			tmp.positionData = position;
			// Debug.Log(position);
			positionMap.Add(position, o); 
			nodeMap.Add(position, tmp); 
		}
		allNodesInScene = nodeMap.Values.ToList();
	}
	
	public void CreateAdjacencyMatrix() {
		foreach(Node n in allNodesInScene) {
			n.neighbours = GetNeighbours(n.positionData, new List<Node>(){n});
		}
	}

	public List<Node> GetNeighbours(Vector3 cell, List<Node> neighbours) {
	neighbours.Clear();

		for (int dx = -1; dx <= 1; dx++) {
			for (int dy = -1; dy <= 1; dy++) {
				for (int dz = -1; dz <= 1; dz++) {
					Vector3 coord = cell + new Vector3(dx, dy, dz);

					if (!nodeMap.ContainsKey(coord)) {
						continue;
					}
					
					bool notSelf = !(dx == 0 && dy == 0 && dz == 0);
					bool connectivity = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) <= 2;

					if (notSelf && connectivity) {
						neighbours.Add(nodeMap[coord]);
					}
				}
			}
		}
		return neighbours;
	}
}

#if UNITY_EDITOR

[CustomEditor(typeof(Path3D))]
[CanEditMultipleObjects]
public class Path3D_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (GUILayout.Button("Start-PathFinding"))
		{
			foreach (var targ in targets)
			{
				((Path3D)targ).StartThisMethod();
			}
		}
	}
}

#endif