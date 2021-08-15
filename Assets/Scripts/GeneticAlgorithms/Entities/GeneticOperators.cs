using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using Random = System.Random;

namespace GeneticAlgorithms.Entities
{
    [Serializable]
    public class GeneticOperators
    {
        private Random _random;
        private int _k;
    
        public GeneticOperators(Random random, int k)
        {
            _random = random;
            _k = k;
        }

        public Chromosome FitnessProportionateSelection(List<Chromosome> population, float sumOfFitness)
        {
            // deals with negative and positive values
            float minimalFitness = 0.0f;
            for (int i = 0; i < population.Count; ++i)
            {
                var score = population[i].Fitness;
                if (score < minimalFitness)
                    minimalFitness = score;
            }

            minimalFitness = Math.Abs(minimalFitness);
            sumOfFitness += minimalFitness * population.Count;
            
            var val = _random.NextDouble() * sumOfFitness;

            var value = 0f;

            for (int i = 0; i < population.Count; ++i)
            {
                value += population[i].Fitness + minimalFitness + 0.02f;
                if (value > val)
                {
                    return population[i];
                }
            }
            return population[_random.Next(0,population.Count)];
        }
        public List<Chromosome> SinglePointCrossover(Chromosome chromosome1, Chromosome chromosome2)
        {
            int randomPosition = _random.Next(0, chromosome1.Genes.Count);
        
            List<Gene> newChromosome1 = chromosome1.Genes
                .GetRange(0, randomPosition)
                .Concat(chromosome2.Genes.GetRange(randomPosition, chromosome2.Genes.Count() - randomPosition))
                .ToList();
        
            List<Gene> newChromosome2 = chromosome2.Genes
                .GetRange(0, randomPosition)
                .Concat(chromosome1.Genes.GetRange(randomPosition, chromosome1.Genes.Count() - randomPosition))
                .ToList();

            Chromosome chomesome1 = new Chromosome();
            chomesome1.Genes = newChromosome1;
            Chromosome chomesome2 = new Chromosome();
            chomesome2.Genes = newChromosome2;

            return new List<Chromosome> { chomesome1, chomesome2 };
        }
    
        public List<Chromosome> TournamentSelection(List<Chromosome> population)
        {
            List<Chromosome> selections = new List<Chromosome>();

            HashSet<Chromosome> unique = new HashSet<Chromosome>();
            List<Chromosome> possibleParents1 = new List<Chromosome>();
            List<Chromosome> possibleParents2 = new List<Chromosome>();
            // ensure that the population has at least k individuals to pick possible parents
            // if not just return all elements
            if (population.Count() < _k)
            {
                return selections;
            }

            while (unique.Count <= _k)
            {
                possibleParents1 = Utility.GetKRandomElements(population, _k, _random);
                foreach (var chromosome in possibleParents1) unique.Add(chromosome);
                possibleParents2 = Utility.GetKRandomElements(population, _k, _random);
                foreach (var chromosome in possibleParents2) unique.Add(chromosome);
                if (unique.Count <= _k)
                {
                    unique.Clear();
                }
            }

            List<Chromosome> topParent1 = Utility.FindNBestFitness_ByChromosome(possibleParents1, 1);
            List<Chromosome> topParent2 = Utility.FindNBestFitness_ByChromosome(possibleParents2, 1);
        
            selections.Add(topParent1[0]);
            selections.Add(topParent2[0]);

            // return 2 parents
            return selections;
        }
    
        public Chromosome UniformMutation(Chromosome chromosome, double probability, Func<Gene> generateGene)
        {
            for(int i=0; i < chromosome.Genes.Count; i++)
            {
                // replaces a gene with random new gene.
                if (!(_random.NextDouble() < probability)) continue;
            
                chromosome.Genes[i] = generateGene();
            }

            return chromosome;
        }
    }
}