using System;
using System.Collections.Generic;
using System.Linq;
using Behaviour.Entities;
using GeneticAlgorithms.Parameter;
using PathFinding.Entities;
using UnityEngine;
using Utilities;

namespace PathFinding
{
#if UNITY_EDITOR

#endif
	public class PathFinding3D: MonoBehaviour {
		
		[SerializeField]
		private GameObject platform;

		private Dictionary<Vector3, Node> _nodeMap = new Dictionary<Vector3, Node>();
		private List<Node> _allWalkableNodesInScene = new List<Node>();
		private Node _player;
		private Node _target;
		private List<Vector3> _walkableSurface;
		
		public List<List<Vector3>> FindPaths()
		{
			ResetPathFinding();

			if (Utility.GetGameMap().Count == 0)
			{
				throw new Exception("game map has not been made");
			}
		
			FindAllWalkableNodes();
			CreateAdjacencyMatrix();

			_player = _nodeMap[Utility.GetGameMap()[BlockType.AGENT][0]];
			_target = _nodeMap[Utility.GetGameMap()[BlockType.GOAL][0]];
			if (_player == null || _target == null)
			{
				throw new Exception("No Player or target is found");
			}
			
			var p = new GraphPaths(_player, _target);
			return p.FindAllUniquePaths();
		}

		private void FindAllWalkableNodes() {
			_walkableSurface = Utility.GetGameMap()[BlockType.FREE_TO_WALK];

			foreach (Transform child in platform.transform)
			{
				var position = child.position;
				if (!_walkableSurface.Contains(position)) continue;
				var tmp = (child.gameObject).GetComponent<Node>();
				tmp.positionData = position;
				tmp.walkable = true;
				_nodeMap.Add(position, tmp); 
			}
			_allWalkableNodesInScene = _nodeMap.Values.ToList();
		}
	
		private void CreateAdjacencyMatrix() {
			foreach(var n in _allWalkableNodesInScene) {
				n.neighbours = GetNeighbours(n.positionData, new List<Node>(){n});
			}
		}

		private List<Node> GetNeighbours(Vector3 cell, List<Node> neighbours) {
			neighbours.Clear();
			for (var dx = -1f; dx <= 1f; dx = dx + Constants.BLOCK_SIZE) {
				for (var dy = -0.5f; dy <= 0.5f; dy = dy + Constants.BLOCK_SIZE) {
					for (var dz = -1f; dz <= 1f;dz = dz + Constants.BLOCK_SIZE) {
						var coord = cell + new Vector3(dx, dy, dz);
						if (!_nodeMap.ContainsKey(coord)) continue;
						var tmp = _nodeMap[coord];

						var notSelf = !(dx == 0 && dy == 0 && dz == 0);

						var connectivity = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) <= 2.5;
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
			_allWalkableNodesInScene = new List<Node>();
			_walkableSurface = new List<Vector3>();
		}
	}
}