using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

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

        // private Func<Chromosome, float> fitnessFunction;

        public float fitness { get; set; }

        public FeatureDimension featureDescriptor;

        public Chromosome(List<Gene> g)
        {
            genes = g;
        }

        public Chromosome()
        {
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

    [Serializable]
    public class FeatureDimension
    {
        public static Dictionary<string, Func<Chromosome, float>> featureFunctionMap = 
            new Dictionary<string, Func<Chromosome, float>>()
            {
                {"NumberOfJumps",NumberOfJumps},
                {"LengthOfPath", LengthOfPath},
                {"NumberOfTurns", NumberOfTurns}
            };
        
        public string featureName;
        private Func<Chromosome, float> featureFunction;
        public float target;
        // fix output of this. This is suppose to be a operator
        public int compareFeature;
        public int[] bins;
        
        public FeatureDimension(string featureName)
        {
            this.featureName = featureName;
            this.featureFunction = featureFunctionMap[featureName];
        }

        public float Calculate(Chromosome chromosome)
        {
            return Math.Abs(this.featureFunction(chromosome) - target);
        }

        public int Discretize(float value)
        {
            int index = RetrieveBin(value);
            return index;
        }

        private int RetrieveBin(float val)
        {
            return 0;
        }

        private static float NumberOfJumps(Chromosome chromosome)
        {
            return 0.0f;
        }
        
        private static float LengthOfPath(Chromosome chromosome)
        {
            return 0.0f;
        }
        
        private static float NumberOfTurns(Chromosome chromosome)
        {
            return 0.0f;
        }
        
    }
    
    [Serializable]
    public class Operators
    {
        private Random _random;
        private int _k;
        
        public Operators(Random random, int k)
        {
            this._random = random;
            this._k = k;
        }

        public List<Chromosome> SinglePointCrossover(Chromosome chromosome1, Chromosome chromosome2)
        {
            int randomPosition = _random.Next(0, chromosome1.genes.Count);
            
            List<Gene> newChromosome1 = chromosome1.genes
                .GetRange(0, randomPosition)
                .Concat(chromosome2.genes.GetRange(randomPosition, chromosome2.genes.Count() - randomPosition))
                .ToList();
            
            List<Gene> newChromosome2 = chromosome2.genes
                .GetRange(0, randomPosition)
                .Concat(chromosome1.genes.GetRange(randomPosition, chromosome1.genes.Count() - randomPosition))
                .ToList();

            Chromosome chomesome1 = new Chromosome();
            chomesome1.genes = newChromosome1;
            Chromosome chomesome2 = new Chromosome();
            chomesome2.genes = newChromosome2;

            return new List<Chromosome> { chomesome1, chomesome2 };
        }
        
        public List<Chromosome> TournamentSelection(List<Chromosome> population)
        {
            List<Chromosome> selections = new List<Chromosome>();
            
            // ensure that the population has at least k individuals to pick possible parents
            // if not just return all elements
            if (population.Count() < _k)
            {
                return selections;
            }

            List<Chromosome> possibleParents1 = Utility.GetKRandomElements(population, _k, _random);
            List<Chromosome> possibleParents2 = Utility.GetKRandomElements(population, _k, _random);

            List<Chromosome> topParent1 = Utility.FindNBestFitness_ByChromosome(possibleParents1, 1);
            List<Chromosome> topParent2 = Utility.FindNBestFitness_ByChromosome(possibleParents2, 1);
            
            selections.Add(topParent1[0]);
            selections.Add(topParent2[0]);

            // return 2 parents
            return selections;
        }
        
        public Chromosome UniformMutation(Chromosome chromosome, double probability, Func<Gene> generateGene)
        {
            // replaces a gene with random new gene.
            if (_random.NextDouble() < probability)
            {
                List<Gene> genes = chromosome.genes;
                int rv = _random.Next(0, chromosome.genes.Count);
                
                genes[rv] = generateGene();
                chromosome.genes = genes;
            }
            return chromosome;
        }
    }
}
