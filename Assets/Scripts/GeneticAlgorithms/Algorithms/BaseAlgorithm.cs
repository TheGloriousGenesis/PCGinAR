using System;
using System.Collections.Generic;
using Behaviour.Entities;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Parameter;
using UnityEngine;
using Utilities;
using Random = System.Random;

namespace GeneticAlgorithms.Algorithms
{
    // TODO: Check that a given chromosone has unique genes that do not overlap in space (maybe use hashset)
    public abstract class BaseAlgorithm
    {
        protected readonly int PopulationSize;
        protected int Iteration; // same as number of generations
        protected readonly int ChromosoneLength;
        protected readonly float CrossoverProbability;
        protected readonly float MutationProbability;
        protected readonly Random RandomG;
        protected int Elitism;
        protected readonly GeneticOperators GeneticGeneticOperator;

        protected BaseAlgorithm(int populationSize, int chromosoneLength, float crossoverProbability, Random randomG, 
            int elitism, float mutationProbability, int iteration, int k)
        {
            this.PopulationSize = populationSize;
            ChromosoneLength = chromosoneLength;
            CrossoverProbability = crossoverProbability;
            RandomG = randomG;
            Elitism = elitism;
            MutationProbability = mutationProbability;
            Iteration = iteration;
            // create initial population
            GeneticGeneticOperator = new GeneticOperators(RandomG, k);

        }
    
        #region Core genetic infrastucture (DNA)
        protected List<Chromosome> GenerateGenotype(int popSize)
        {
            // A Genotype is the population in computation space
            var genotype = new List<Chromosome>();
            for (var i=0; i < popSize; i++)
            {
                genotype.Add(GenerateChromosome());
            }
            return genotype;
        }

        private Chromosome GenerateChromosome()
        {
            // a genotype is a solution to the level. feed in number of blocks and restrictions to generate possible level
            var genes = new List<Gene>();

            // todo: check how many genes in chromosone good fit
            for (var i=0; i < ChromosoneLength; i++)
            {
                genes.Add(GenerateGene());
            }

            var chromosome = new Chromosome {Genes = genes};
            return chromosome;
        }

        protected Gene GenerateGene()
        {
            return new Gene(GenerateAllele());
        }

        // Generates a 3 cubed structure as a single gene
        private Allele GenerateAllele()
        {
            var cubeNumbers = new List<Vector3> { BlockPosition.UP, BlockPosition.DOWN,
                BlockPosition.LEFT, BlockPosition.RIGHT, BlockPosition.FRONT, BlockPosition.BACK};

            Vector3 centerBlock = GenerateCenterBlockPosition();

            var positions = new List<Vector3> {centerBlock};

            for (var i = 0; i < 2; i++)
            {
                // check if it is inclusive?
                var choosePosition = RandomG.Next(0, cubeNumbers.Count);
                positions.Add(cubeNumbers[choosePosition] + centerBlock);
                cubeNumbers.Remove(cubeNumbers[choosePosition]);
            }
        
            return new Allele(positions);
        }

        private Vector3 GenerateCenterBlockPosition()
        {
            // good for condensing the size of the space
            const int maxPlatformLength = Constants.MAX_PLATFORM_DIMENSION / 2;
            var xPos = RandomG.Next(0, maxPlatformLength);
            var yPos = RandomG.Next(0, maxPlatformLength);
            var zPos = RandomG.Next(0, maxPlatformLength);
            return new Vector3(xPos, yPos, zPos);
        }
        #endregion
    
        public abstract List<Chromosome> Run(Func<Chromosome, double> fitness);
        public abstract void SaveInfo();
    }
}