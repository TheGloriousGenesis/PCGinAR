using System;
using System.Collections.Generic;
using System.Linq;
using Behaviour.Entities;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Parameter;
using UnityEngine;
using Utilities;
using Random = System.Random;

namespace GeneticAlgorithms.Algorithms
{
    // TODO: Check that a given chromosome has unique genes that do not overlap in space (maybe use hashset)
    public abstract class BaseAlgorithm
    {
        public readonly int ChromosomeLength;
        public string Variation;
        
        protected readonly int PopulationSize;
        protected int Iteration; // same as number of generations
        protected readonly float CrossoverProbability;
        protected readonly float MutationProbability;
        protected readonly float PlatformWidth;
        protected readonly float PlatformHeight;
        protected readonly float PlatformDepth;
        protected readonly Random RandomG;
        protected int Elitism;
        protected readonly GeneticOperators GeneticGeneticOperator;
        protected float[] currentWeights;

        public WeightedRandomBag<int> weightedRandomBag;

        protected BaseAlgorithm(int populationSize, int chromosomeLength, float crossoverProbability, Random randomG, 
            int elitism, float mutationProbability, int iteration, int k,
            int width, int height, int depth)
        {
            this.PopulationSize = populationSize;
            ChromosomeLength = chromosomeLength;
            CrossoverProbability = crossoverProbability;
            RandomG = randomG;
            Elitism = elitism;
            MutationProbability = mutationProbability;
            Iteration = iteration;
            GeneticGeneticOperator = new GeneticOperators(RandomG, k);
            weightedRandomBag = new WeightedRandomBag<int>(randomG, Constants.NUMBER_OF_CHUNKS);
            SetUpCurrentWeights(Constants.NUMBER_OF_CHUNKS);
            PlatformWidth = width;
            PlatformHeight = height;
            PlatformDepth = depth;
        }

        #region Core genetic infrastucture (DNA)

        protected void SetUpCurrentWeights(int numberOfChunks)
        {
            currentWeights = new float[numberOfChunks];
            for (int i = 0; i < numberOfChunks; i++)
            {
                currentWeights[0] = 1f / numberOfChunks;
            }
        }
        
        protected List<Chromosome> GenerateGenotype(int popSize)
        {
            // A Genotype is the population in computation space
            List<Chromosome> genotype = new List<Chromosome>();
            for (var i=0; i < popSize; i++)
            {
                genotype.Add(GenerateChromosome());
            }
            return genotype;
        }

        private Chromosome GenerateChromosome()
        {
            // a genotype is a solution to the level. feed in number of blocks and restrictions to generate possible level
            List<Gene> genes = new List<Gene>();

            // todo: check how many genes in chromosone good fit
            for (var i=0; i < ChromosomeLength; i++)
            {
                genes.Add(GenerateGene());
            }

            Chromosome chromosome = new Chromosome {Genes = genes};
            return chromosome;
        }

        protected Gene GenerateGene()
        {
            return new Gene(GenerateAllele());
        }

        // Generates a 3 cubed structure as a single gene
        private Allele GenerateAllele()
        {
            Vector3 centerBlock = GenerateCenterBlockPosition();

            int chunkID = weightedRandomBag.GetRandom();
            
            return new Allele(centerBlock, chunkID);
        }

        protected void UpdateWeights()
        {
            weightedRandomBag.UpdateWeights(currentWeights);
        }

        private Vector3 GenerateCenterBlockPosition()
        {
            float[] xRange = Utility.Range(0f, PlatformWidth, Constants.BLOCK_SIZE);
            float[] yRange = Utility.Range(0f, PlatformHeight, Constants.BLOCK_SIZE);
            float[] zRange = Utility.Range(0f, PlatformDepth, Constants.BLOCK_SIZE);
            //todo analyse the changes for this
            // good for condensing the size of the space
            var xPos = RandomG.Next(xRange.Length);
            var yPos = RandomG.Next(yRange.Length);
            var zPos = RandomG.Next(zRange.Length);
            return new Vector3(xRange[xPos], yRange[yPos], zRange[zPos]);
        }
        #endregion
    
        public abstract List<Chromosome> Run(Func<Chromosome, FitnessValues> fitness);
        public abstract void SaveInfo();
    }
}