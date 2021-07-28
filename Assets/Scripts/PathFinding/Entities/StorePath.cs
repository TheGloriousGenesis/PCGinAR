using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathFinding.Entities
{
    [Serializable]
    public class StorePath
    {
        private readonly StorePath parent;
        public readonly Node current;

        public StorePath(Node current, StorePath parent)
        {
            this.current = current;
            this.parent = parent;
        }
	
        public bool Seen(Node node)
        {
            if(current == node)
                return true;
            return parent != null && parent.Seen(node);
        }
	
        public List<Vector3> GeneratePath() {
            var path = parent != null ? parent.GeneratePath() : new List<Vector3>();
            path.Add(current.positionData);
            current.usedInPreviousPath = true;
            return path;
        }
	
    }
}