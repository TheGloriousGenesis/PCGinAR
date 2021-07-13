using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR

#endif

public class StorePath
{
	public Vector3 current;
	public StorePath next;

	public StorePath(Vector3? _current)
	{
		if (_current != null) this.current = (Vector3) _current;
	}
}
public class GraphPrintAllPaths {

	private List<StorePath> allPaths = new List<StorePath>();
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
 
	//  public void print(Node start, Node end, StorePath path, Dictionary<Node, Boolean> visited){
	// 	 // StorePath newPath = new StorePath(path.current);
	// 	 path.next = new StorePath(start.positionData);
	// 	 visited[start] = true;
	// 	 List<Node> list = start.neighbours;
	// 	 for (int i = 0; i < list.Count ; i++) {
	// 		 Node node = list[i];
	// 		 if(node!=end && visited[node]==false){
	// 			 print(node,end,path.next,visited);
	// 		 }else if(node==end){
	// 			 // StorePath tmp = new StorePath(start.positionData, node.positionData);
	// 			 allPaths.Add(path);
	// 		 }
	// 	 }
	// 	 //remove from path
	// 	 visited[start] = false;
	//  }
 //
	// public void printAllPaths(List<Node> graph, Node start, Node end){
	// 	Dictionary<Node, Boolean> visited = graph.ToDictionary(x => x, x => false);
 //        visited[start] = true;
 //        StorePath path = new StorePath(null);
 //        print(start, end, path, visited);
 //    }

	public List<StorePath> GetAllPaths()
	{
		return allPaths;
	}
}

public class Path3D: MonoBehaviour {

	#region graph variables
	private Dictionary<Vector3, GameObject> positionMap = new Dictionary<Vector3, GameObject>();
	private Dictionary<Vector3, Node> nodeMap = new Dictionary<Vector3, Node>();
	private List<Node> allNodesInScene = new List<Node>();
	#endregion

	private List<Vector3> walkableSurface;
	[SerializeField]
	private GameObject platform;
	[SerializeField]
	private Node player;
	[SerializeField]
	private Node target;
	public void StartThisMethod()
	{
		ResetPathFinding();
		Debug.Log("Starting Pathfinding..");
		// GetWalkable();
		walkableSurface = Utility.gamePlacement.Keys.ToList();
		Debug.Log($"Walkable area has been found:  {walkableSurface.Count}");
		FindAllNodes();
		Debug.Log("Nodes in scene found..");
		CreateAdjacencyMatrix();
		Debug.Log("Adjacency Matrix created..");
		GraphPrintAllPaths p = new GraphPrintAllPaths();
		player = nodeMap[Utility.GetKeyFromValue(Utility.gamePlacement, BlockType.PLAYER)[0]];
		target = nodeMap[Utility.GetKeyFromValue(Utility.gamePlacement, BlockType.GOAL)[0]];
		Debug.Log($"Start Node position..{player.positionData}");
		Debug.Log($"End Node position..{target.positionData}");
		Stopwatch timer = new Stopwatch();
		timer.Start();
		p.printAllPaths(allNodesInScene,player,target);
		timer.Stop();
		
		Debug.Log($"Time elapsed: {timer.Elapsed.TotalMilliseconds}");
		Debug.Log($"Number of paths: {p.GetAllPaths().Count}");
	}

	private void ResetPathFinding()
	{
	positionMap = new Dictionary<Vector3, GameObject>();
	nodeMap = new Dictionary<Vector3, Node>();
	allNodesInScene = new List<Node>();
	walkableSurface = new List<Vector3>();
	}

	public void FindAllNodes() {
		foreach (Transform child in platform.transform)
		{
			Vector3 position = child.position;
			GameObject o;
			Node tmp = (o = child.gameObject).GetComponent<Node>();
			tmp.positionData = position;
			if (walkableSurface.Contains(position))
			{
				tmp.walkable = true;
			}
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

	// private void GetWalkable()
	// {
	// 	foreach (Vector3 child in walkableSurface)
	// 	{
	// 		Debug.DrawLine(child, child + Vector3.up * 4, Color.green, 5);
	// 	}
	// }

	public List<Node> GetNeighbours(Vector3 cell, List<Node> neighbours) {
	neighbours.Clear();

		for (int dx = -2; dx <= 2; dx++) {
			for (int dy = -2; dy <= 2; dy++) {
				for (int dz = -2; dz <= 2; dz++) {
					Vector3 coord = cell + new Vector3(dx, dy, dz);

					if (!nodeMap.ContainsKey(coord)) {
						continue;
					}

					Node tmp = nodeMap[coord];

					bool notSelf = !(dx == 0 && dy == 0 && dz == 0);
					bool connectivity = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) <= 2;

					if (notSelf && connectivity && tmp.walkable) {
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