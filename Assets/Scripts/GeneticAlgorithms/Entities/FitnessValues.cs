using UnityEngine;
using UnityEngine.AI;

namespace GeneticAlgorithms.Entities
{
    public class FitnessValues
    {
        public NavMeshPathStatus pathStatus;
        public double time { get; set; }
        public float fitness { get; set; }
        public double linearity { get; set; }
        public float pathLength { get; set; }
        public float nullSpace { get; set; }
        public float walkableSurface { get; set; }
        public int numberOfPaths { get; set; }
    }
}
