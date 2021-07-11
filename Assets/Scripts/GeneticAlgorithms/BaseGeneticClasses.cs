using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaseGeneticClass
{

    [Serializable]
    public class Allele
    {
        //public Vector3 centerBlock { get; set; }

        [SerializeField]
        public List<Vector3> blockPositions = new List<Vector3>();

        public Allele(List<Vector3> positions)
        {
            blockPositions.AddRange(positions);
        }

        public Allele()
        {

        }

    }

    [Serializable]
    public class Gene
    {
        public Allele allele { get; set; }

        public Gene (Allele _allele)
        {
            allele = _allele;
        }
    }

    [Serializable]
    public class Chromosome
    {
        public int id_ { get; set; }
        public List<Gene> genes { get; set; }

        private Func<Chromosome, float> fitnessFunction;

        public float fitness { get; set; }

        public Chromosome(List<Gene> g)
        {
            genes = g;
        }

        public Chromosome()
        {
        }

        public float CalculateFitness()
        {
            fitness = fitnessFunction(this);
            return fitness;
        }

        public Chromosome DeepCopy()
        {
            Chromosome chromosome = (Chromosome)this.MemberwiseClone();
            List<Gene> _genes = new List<Gene>();
            foreach (Gene i in genes)
            {
                _genes.Add(new Gene(i.allele));
            }
            chromosome.genes = _genes;
            return chromosome;
        }
    }

}
