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
		private List<Node> allWalkableNodesInScene = new List<Node>();
		#endregion
	
		[FormerlySerializedAs("_walkableSurface")] [SerializeField]
		private List<Vector3> walkableSurface;
		[SerializeField]
		private GameObject platform;
		[SerializeField]
		private Node player;
		[SerializeField]
		private Node target;
		
		public List<List<Vector3>> FindPaths()
		{
			ResetPathFinding();

			if (Utility.GetGameMap().Count == 0)
			{
				throw new Exception("game map has not been made");
			}
		
			FindAllWalkableNodes();
			CreateAdjacencyMatrix();

			player = _nodeMap[Utility.GetGameMap()[BlockType.AGENT][0]];
			target = _nodeMap[Utility.GetGameMap()[BlockType.GOAL][0]];
			
			if (player == null || target == null)
			{
				throw new Exception("No Player or target is found");
			}
			
			var p = new GraphPaths(player, target);
			return p.FindAllUniquePaths();
		}

		private void FindAllWalkableNodes() {
			walkableSurface = Utility.GetGameMap()[BlockType.FREE_TO_WALK];

			foreach (Transform child in platform.transform)
			{
				var position = child.position;
				if (!walkableSurface.Contains(position)) continue;
				var tmp = (child.gameObject).GetComponent<Node>();
				tmp.positionData = position;
				tmp.walkable = true;
				_nodeMap.Add(position, tmp); 
			}
			allWalkableNodesInScene = _nodeMap.Values.ToList();
		}
	
		private void CreateAdjacencyMatrix() {
			foreach(var n in allWalkableNodesInScene) {
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
		
		public void ResetPathFinding()
		{
			_nodeMap = new Dictionary<Vector3, Node>();
			allWalkableNodesInScene = new List<Node>();
			walkableSurface = new List<Vector3>();
		}
	}
}