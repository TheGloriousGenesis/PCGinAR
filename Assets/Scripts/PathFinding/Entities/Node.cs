using System.Collections.Generic;
using UnityEngine;

namespace PathFinding
{
    public class Node: MonoBehaviour {
        public Vector3 positionData;
	
        public List<Node> neighbours;

        public bool walkable;

        public bool usedInPreviousPath;
    }
}