using System.Collections.Generic;
using UnityEngine;

namespace PathFinding.Entities
{
    public class GraphPaths
    {
        private readonly Node _destination;
        private readonly Queue<StorePath> _pending;

        public GraphPaths(Node source, Node destination)
        {
            _destination = destination;
            _pending = new Queue<StorePath>();
            _pending.Enqueue(new StorePath(source, null));
        }

        // Breadth-First-Search
        public List<Vector3> NextShortestPath()
        {
            var current = _pending.Dequeue();
            while (current != null)
            {
                if (current.current == _destination)
                {
                    return current.GeneratePath();
                }
                foreach (var n in current.current.neighbours)
                {
                    if (!current.Seen(n) && n.usedInPreviousPath != true)
                    {
                        var nextPath = new StorePath(n, current);
                        _pending.Enqueue(nextPath);
                    }
                }
                current = _pending.Count == 0 ? null : _pending.Dequeue();
            }
            return null;
        }
    }
}