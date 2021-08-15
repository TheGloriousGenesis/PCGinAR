using System;
using System.Collections.Generic;
using System.Threading;

namespace GeneticAlgorithms.Entities
{
    [Serializable]
    public class Chromosome
    {
        private static int _mCounter;
        public int ID { get; set; }
        public List<Gene> Genes = new List<Gene>();
    
        public float Fitness { get; set; }

        public Chromosome(List<Gene> g)
        {
            Genes = g;
            ID = Interlocked.Increment(ref _mCounter);

        }

        public static void ResetCounter()
        {
            _mCounter = 0;
        }

        public Chromosome()
        {
            ID = Interlocked.Increment(ref _mCounter);
        }
    }
}