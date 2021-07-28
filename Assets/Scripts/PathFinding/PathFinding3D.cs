using System;
using System.Collections.Generic;
using System.Linq;
using Behaviour.Entities;
using PathFinding.Entities;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

namespace PathFinding
{
#if UNITY_EDITOR

#endif
	public class PathFinding3D: MonoBehaviour {

		#region graph variables
		private Dictionary<Vector3, Node> _nodeMap = new Dictionary<Vector3, Node>();
		private List<Node> _allNodesInScene = new List<Node>();
		#endregion
	
		[FormerlySerializedAs("_walkableSurface")] [SerializeField]
		private List<Vector3> walkableSurface;
		[SerializeField]
		private GameObject platform;
		[SerializeField]
		private Node player;
		[SerializeField]
		private Node target;

		private HashSet<List<Vector3>> _allPaths = new HashSet<List<Vector3>>();
	
		public List<List<Vector3>> FindPaths()
		{
			ResetPathFinding();

			walkableSurface = Utility.GamePlacement.Keys.ToList();
			if (walkableSurface.Count == 0)
			{
				throw new Exception("game map has not been made");
			}
		
			FindAllNodes();
			CreateAdjacencyMatrix();

			player = _nodeMap[Utility.GetKeyFromValue(Utility.GamePlacement, BlockType.PLAYER)[0]];
			target = _nodeMap[Utility.GetKeyFromValue(Utility.GamePlacement, BlockType.GOAL)[0]];
			var p = new GraphPaths(player, target);
			var tmp = p.NextShortestPath();
			_allPaths.Add(tmp);
			var count = 20;
			while(tmp != null && count > 0) {
				tmp = p.NextShortestPath();
				_allPaths.Add(tmp);
				count--;
			}
			return _allPaths.ToList();
		}

		public void ResetPathFinding()
		{
			_nodeMap = new Dictionary<Vector3, Node>();
			_allNodesInScene = new List<Node>();
			walkableSurface = new List<Vector3>();
			_allPaths = new HashSet<List<Vector3>>();
		}

		private void FindAllNodes() {
			foreach (Transform child in platform.transform)
			{
				var position = child.position;
				var tmp = (child.gameObject).GetComponent<Node>();
				tmp.positionData = position;
				if (walkableSurface.Contains(position))
				{
					tmp.walkable = true;
				}
				_nodeMap.Add(position, tmp); 
			}
			_allNodesInScene = _nodeMap.Values.ToList();
		}
	
		private void CreateAdjacencyMatrix() {
			foreach(var n in _allNodesInScene) {
				n.neighbours = GetNeighbours(n.positionData, new List<Node>(){n});
			}
		}

		private List<Node> GetNeighbours(Vector3 cell, List<Node> neighbours) {
			neighbours.Clear();
			for (var dx = -2; dx <= 2; dx++) {
				for (var dy = -1; dy <= 1; dy++) {
					for (var dz = -2; dz <= 2; dz++) {
						var coord = cell + new Vector3(dx, dy, dz);

						if (!_nodeMap.ContainsKey(coord)) continue;
						var tmp = _nodeMap[coord];

						var notSelf = !(dx == 0 && dy == 0 && dz == 0);

						var connectivity = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) <= 5;
						if (notSelf && connectivity && tmp.walkable) {
							neighbours.Add(_nodeMap[coord]);
						}
					}
				}
			}
			return neighbours;
		}
	}
//
// #if UNITY_EDITOR
//
// 	[CustomEditor(typeof(Path3D))]
// 	[CanEditMultipleObjects]
// 	public class Path3DEditor : Editor
// 	{
// 		public override void OnInspectorGUI()
// 		{
// 			DrawDefaultInspector();
//
// 			if (!GUILayout.Button("Start-PathFinding")) return;
// 			foreach (var targ in targets)
// 			{
// 				((Path3D)targ).FindPaths();
// 			}
// 		}
// 	}
// #endif
}