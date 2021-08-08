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
        
        public List<List<Vector3>> FindAllUniquePaths()
        {
            List<List<Vector3>> allPaths = new List<List<Vector3>>();
            
            var tmp = NextShortestPath();
            var count = 20;

            while (tmp != null && count > 0)
            {
                tmp = NextShortestPath();
                allPaths.Add(tmp);
                count--;
            }

            return allPaths;
        }
        
        // Breadth-First-Search
        private List<Vector3> NextShortestPath()
        {
            while (_pending.Count > 0)
            {
                var current = _pending.Dequeue();

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
            }
            return null;
        }

        
    }
}