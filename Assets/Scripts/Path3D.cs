using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.Ast;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR

#endif

public class StorePath
{
	public StorePath parent;
	public Node current;

	// public StorePath(Vector3? current)
	// {
	// 	if (current != null) this.current = (Vector3) current;
	// }

	public StorePath(Node current, StorePath parent)
	{
		this.current = current;
		this.parent = parent;
	}
	
	public bool seen(Node node) {
		if(this.current == node)
			return true;
		else if(parent == null)
			return false;
		else
			return this.parent.seen(node);
	}
	
	public List<Vector3> generatePath() {
		List<Vector3> path;
		if (this.parent != null)
			path = this.parent.generatePath();
		else
			path = new List<Vector3>();
		path.Add(current.positionData);
		return path;
	}
	
}
public class GraphPrintAllPaths
{
	// private Dictionary<Node, List<Node>> adjacency;
	private Node destination;
	Queue<StorePath> pending;

	public GraphPrintAllPaths(Node source, Node destination)
	{
		this.destination = destination;
		this.pending = new Queue<StorePath>();
		this.pending.Enqueue(new StorePath(source, null));
	}

	public GraphPrintAllPaths()
	{
		
	}
	
	// bfs
	public List<Vector3> nextShortestPath()
	{
		StorePath current = this.pending.Dequeue();
		while (current != null)
		{
			if (current.current == destination)
			{
				return current.generatePath();
			}

			foreach (Node n in current.current.neighbours)
			{
				if (!current.seen(n))
				{
					StorePath nextPath = new StorePath(n, current);
					this.pending.Enqueue(nextPath);
				}
			}

			if (this.pending.Count == 0)
			{
				current = null;
			}
			else
			{
				current = this.pending.Dequeue();
			}
		}
		return null;
	}
	// private List<List<Vector3>> allPaths = new List<List<Vector3>>();
	//
	// public void print_(Node start, Node end, string path, Dictionary<Node, Boolean> visited){
 //        String newPath = path +  "->" + start.positionData;
 //        visited[start] = true;
 //        List<Node> list = start.neighbours;
 //        for (int i = 0; i < list.Count ; i++) {
 //            Node node = list[i];
 //            if(node!=end && visited[node]==false){
 //                print_(node,end,newPath,visited);
 //            }else if(node==end){
 //                Debug.Log(newPath + "->" + node.positionData);
 //            }
 //        }
 //        //remove from path
 //        visited[start] = false;
 //    }
 //
	//  public void printAllPaths_(List<Node> graph, Node start, Node end){
	// 	 Dictionary<Node, Boolean> visited = graph.ToDictionary(x => x, x => false);
	// 	 visited[start] = true;
	// 	 print_(start, end, "", visited);
	//  }
 //
	//  public void print(Node start, Node end, Dictionary<Node, Boolean> visited,
	// 	 List<Vector3> path, List<List<Vector3>> allResults)
	//  {
	// 	 path.Add(start.positionData);
	// 	 visited[start] = true;
	// 	 List<Node> list = start.neighbours;
	// 	 for (int i = 0; i < list.Count; i++)
	// 	 {
	// 		 Node node = list[i];
	// 		 if (node != end && visited[node] == false)
	// 		 {
	// 			 print(node, end, visited, path, allResults);
	// 		 }
	// 		 else if (node == end)
	// 		 {
	// 			 path.Add(node.positionData);
	// 			 allResults.Add(path);
	// 			 // path = new List<Vector3>();
	// 			 path.RemoveAt(path.Count - 1);
	// 		 }
	// 	 }
	//  
	// 	 //remove from path
	// 	 visited[start] = false;
	// 	 // path.RemoveAt(path.Count - 1);
	//  }
	//  
	//  public void printAllPaths(List<Node> graph, Node start, Node end){
	// 	 Dictionary<Node, Boolean> visited = graph.ToDictionary(x => x, x => false);
	// 	 visited[start] = true;
	// 	 print(start, end, visited, new List<Vector3>(), allPaths);
	//  }
	//  
	//  public List<List<Vector3>> GetAllPaths()
	// {
	// 	return allPaths;
	// }
	// private Stack<Vector3> path  = new Stack<Vector3>();   // the current path
	// private HashSet<Vector3> onPath  = new HashSet<Vector3>();     // the set of vertices on the path
	// public List<HashSet<Vector3>> allPaths = new List<HashSet<Vector3>>();
	// // use DFS
	// public void enumerate(Node v, Node t) {
	//
	// 	// add node v to current path from s
	// 	path.Push(v.positionData);
	// 	onPath.Add(v.positionData);
	//
	// 	// found path from s to t - currently prints in reverse order because of stack
	// 	if (v.positionData.Equals(t.positionData))
	// 	{
	// 		Debug.Log("-----------");
	// 		foreach (var VARIABLE in path)
	// 		{
	// 			Debug.Log(VARIABLE);
	// 		}
	// 		Debug.Log("-----------");
	// 	}
	// 	// consider all neighbors that would continue path with repeating a node
	// 	else
	// 	{
	// 		List<Node> neighbours = v.neighbours;
	// 		foreach (Node w in neighbours)
	// 		{
	// 			if (!onPath.Contains(w.positionData))
	// 			{
	// 				enumerate( w, t);
	// 			}
	// 		}
	// 	}
	//
	// 	// done exploring from v, so remove from path
	// 	path.Pop();
	// 	onPath.Remove(v.positionData);
	// }	
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

	private HashSet<List<Vector3>> allPaths = new HashSet<List<Vector3>>();

	[SerializeField] 
	public List<Vector3> holder;
	public List<List<Vector3>> FindPaths()
	{
		ResetPathFinding();
		Debug.Log("Starting Pathfinding..");
		walkableSurface = Utility.gamePlacement.Keys.ToList();

		if (walkableSurface.Count == 0)
		{
			throw new Exception("game map has not been made");
		}
		// Debug.Log($"Walkable area has been found:  {walkableSurface.Count}");
		FindAllNodes();
		// Debug.Log("Nodes in scene found..");
		CreateAdjacencyMatrix();
		// Debug.Log("Adjacency Matrix created..");
		// GraphPrintAllPaths p = new GraphPrintAllPaths();
		player = nodeMap[Utility.GetKeyFromValue(Utility.gamePlacement, BlockType.PLAYER)[0]];
		target = nodeMap[Utility.GetKeyFromValue(Utility.gamePlacement, BlockType.GOAL)[0]];
		GraphPrintAllPaths p = new GraphPrintAllPaths(player, target);

		// Debug.Log($"Start Node position..{player.positionData}");
		// Debug.Log($"End Node position..{target.positionData}");
		// Stopwatch timer = new Stopwatch();
		// timer.Start();
		// p.printAllPaths(allNodesInScene,player,target);
		// p.printAllPaths_(allNodesInScene,player,target);
		// p.enumerate(player,target);
		List<Vector3> tmp = p.nextShortestPath();
		allPaths.Add(tmp);
		int count = 20;
		while(tmp != null && count > 0) {
			// Debug.Log(string.Join("->", tmp));
			tmp = p.nextShortestPath();
			allPaths.Add(tmp);
			count--;
		}
		// Debug.Log($"Path length: {tmp.Count}");
		// timer.Stop();
		// holder = tmp;
		// Debug.Log($"Time elapsed multipaths: {timer.Elapsed.TotalMilliseconds}");
		// Debug.Log($"Number of paths: {p.GetAllPaths().Count}");
		// Debug.Log($"Number of paths: {p.allPaths.Count}");
		return allPaths.ToList();
	}

	private void ResetPathFinding()
	{
	positionMap = new Dictionary<Vector3, GameObject>();
	nodeMap = new Dictionary<Vector3, Node>();
	allNodesInScene = new List<Node>();
	walkableSurface = new List<Vector3>();
	}

	private void FindAllNodes() {
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
	
	private void CreateAdjacencyMatrix() {
		foreach(Node n in allNodesInScene) {
			n.neighbours = GetNeighbours(n.positionData, new List<Node>(){n});
		}
	}

	private List<Node> GetNeighbours(Vector3 cell, List<Node> neighbours) {
	neighbours.Clear();

		for (int dx = -2; dx <= 2; dx++) {
			for (int dy = -1; dy <= 1; dy++) {
				for (int dz = -2; dz <= 2; dz++) {
					Vector3 coord = cell + new Vector3(dx, dy, dz);

					if (nodeMap.ContainsKey(coord)) {
						Node tmp = nodeMap[coord];

						bool notSelf = !(dx == 0 && dy == 0 && dz == 0);
						// change this line to fix issue
						// bool connectivity = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) <= 2;
						
						//experiment1 - this changed it hang = too many operations
						// bool connectivity = true;
						//experiment2 - works
						bool connectivity = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) <= 5;
						if (notSelf && connectivity && tmp.walkable) {
							neighbours.Add(nodeMap[coord]);
						}
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
				((Path3D)targ).FindPaths();
			}
		}
	}
}

#endif