using System.Collections.Generic;
using UnityEngine;

public class Node: MonoBehaviour {
    public Vector3 positionData;
	
    public List<Node> neighbours;

    public bool walkable;
}