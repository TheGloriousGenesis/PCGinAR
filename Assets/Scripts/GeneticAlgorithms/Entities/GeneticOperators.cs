﻿using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

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
                List<Gene> genes = chromosome.Genes;
                int rv = _random.Next(0, chromosome.Genes.Count);
            
                genes[rv] = generateGene();
                chromosome.Genes = genes;
            }
            return chromosome;
        }
    }
}