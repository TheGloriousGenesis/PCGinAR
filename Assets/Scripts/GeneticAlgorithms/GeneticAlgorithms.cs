using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BaseGeneticClass;
using LoggerFramework;
using UnityEngine;
using Logger = LoggerFramework.Logger;
using Random = System.Random;

namespace GeneticAlgorithms
{
    // TODO: Check that a given chromosone has unique genes that do not overlap in space (maybe use hashset)
    public abstract class BaseAlgorithm
    {
        protected Chromosome bestSolution;
        protected int _populationSize;
        protected int _iteration; // same as number of generations
        protected int _chromosoneSize;
        protected float _crossoverProbability;
        protected float _mutationProbability;
        protected Random _random;
        protected int _elitism;
        protected int _k;
        protected Operators _operators;

        public BaseAlgorithm(int populationSize, int chromosoneLength, float crossoverProbability, Random random, 
            int elitism, float mutationProbability, int iteration, int k)
        {
            this._populationSize = populationSize;
            this._chromosoneSize = chromosoneLength;
            this._crossoverProbability = crossoverProbability;
            this._random = random;
            this._elitism = elitism;
            this._mutationProbability = mutationProbability;
            this._iteration = iteration;
            this._k = k;
            // create initial population
            this._operators = new Operators(_random, _k);

        }
        
        #region Core genetic infrastucture (DNA) 
        public List<Chromosome> GenerateGenotype(int popSize)
        {
            // A Genotype is the population in computation space
            List<Chromosome> genotype = new List<Chromosome>();
            for (int i=0; i < popSize; i++)
            {
                genotype.Add(GenerateChromosome(i));
            }

            return genotype;
        }

        public Chromosome GenerateChromosome(int id)
        {
            // a genotype is a solution to the level. feed in number of blocks and restrictions to generate possible level
            List<Gene> genes = new List<Gene>();

            // todo: check how many genes in chromosone good fit
            for (int i=0; i < _chromosoneSize; i++)
            {
                genes.Add(GenerateGene());
            }

            Chromosome chromosome = new Chromosome();
            chromosome.id_ = id;
            chromosome.genes = genes;
            return chromosome;
        }

        public Gene GenerateGene()
        {
            return new Gene(GenerateAllele());
        }

        // Generates a 3 cubed structure as a single gene
        public Allele GenerateAllele()
        {
            List<Vector3> cubeNumbers = new List<Vector3> { BlockPosition.UP, BlockPosition.DOWN,
                BlockPosition.LEFT, BlockPosition.RIGHT, BlockPosition.FRONT, BlockPosition.BACK};

            Vector3 centerBlock = GenerateCenterBlockPosition();

            List<Vector3> positions = new List<Vector3>();

            positions.Add(centerBlock);
            for (int i = 0; i < 2; i++)
            {
                // check if it is inclusive?
                int choosePosition = _random.Next(0, cubeNumbers.Count);
                positions.Add(cubeNumbers[choosePosition] + centerBlock);
                cubeNumbers.Remove(cubeNumbers[choosePosition]);
            }
            
            return new Allele(positions);
        }

        private Vector3 GenerateCenterBlockPosition()
        {
            // good for condensing the size of the space
            int maxPlatformLength = Constants.MAX_PLATFORM_DIMENSION / 2;
            int xPos = _random.Next(0, maxPlatformLength);
            int yPos = _random.Next(0, maxPlatformLength);
            int zPos = _random.Next(0, maxPlatformLength);
            return new Vector3(xPos, yPos, zPos);
        }

        #endregion

        public abstract List<Chromosome> Run(Func<Chromosome, double> fitness);
        public abstract void SaveInfo();
    }

    [Serializable]
    public class BasicGeneticAlgorithm : BaseAlgorithm
    {
        private List<Chromosome> currentPopulation;
        private List<Chromosome> runPopulation;
        
        public BasicGeneticAlgorithm(int populationSize, int chromosoneLength, float crossoverProbability, Random random, 
            int elitism, float mutationProbability, int iteration, int k) : 
            base(populationSize, chromosoneLength, crossoverProbability, random, elitism, mutationProbability, iteration,
                k)
        {
            this.currentPopulation = GenerateGenotype(populationSize);
            this.runPopulation = new List<Chromosome>(populationSize);
        }

        public override List<Chromosome> Run(Func<Chromosome, double> fitness)
        {
            runPopulation.Clear();
            
            Chromosome one;
            Chromosome two;
            List<Chromosome> parents;

            double randDouble = 0.0;
            
            for (int iter = 0; iter < _iteration; iter++)
            {
                runPopulation = new List<Chromosome>();

                // calculate fitness for test population.
                for (int i = 0; i < currentPopulation.Count; ++i)
                {
                    double time = fitness(currentPopulation[i]);
                    Logger.Log(LogTarget.BasicGeneticOutput,
                        $"{currentPopulation[i].id_},{time},{currentPopulation[i].fitness},{iter}");
                }
                
                // for last iteration, population does not need to be generated
                if (iter == _iteration - 1) break;

                while (runPopulation.Count < currentPopulation.Count)
                {
                    parents = _operators.TournamentSelection(currentPopulation);
                    one = parents[0];
                    two = parents[1];

                    // determine if crossover occurs.
                    randDouble = _random.NextDouble();
                    if (randDouble <= _crossoverProbability)
                    {
                        List<Chromosome> chromosomes = _operators.SinglePointCrossover(one, two);
                        
                        one = chromosomes[0];
                        two = chromosomes[1];
                    }

                    one = _operators.UniformMutation(one, _mutationProbability,GenerateGene);
                    two = _operators.UniformMutation(two, _mutationProbability,GenerateGene);

                    runPopulation.Add(one);
                    runPopulation.Add(two);
                }

                List<Chromosome> tmpList = currentPopulation;
                currentPopulation = runPopulation;
                runPopulation = tmpList;
            }

            // find best-fitting chromosomes
            currentPopulation.Sort(Utility.CompareChromosome);
            return currentPopulation.Take(5).ToList();
        }

        public override void SaveInfo()
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class MapElites : BaseAlgorithm
    {
        private Chromosome[,,] mapOfElites;
        private double[,,] mapOfElites_fitness;
        // this N must match the feature space variations
        private int N;
        private List<Chromosome> currentPopulation;
        private FeatureDimension[] _featureDimensions;

        public MapElites(int populationSize, int chromosoneLength, float crossoverProbability, Random random, 
            int elitism, float mutationProbability,int iteration, int k, int N) :
            base(populationSize, chromosoneLength, crossoverProbability, random, elitism, 
                mutationProbability, iteration, k)
        {
            this.N = N;
            mapOfElites = new Chromosome[N, N, N];
            mapOfElites_fitness = new double[N, N, N];
            currentPopulation = GenerateGenotype(populationSize);
            _featureDimensions = new FeatureDimension[N];
            InstantiateFeatures();
        }

        private void InstantiateFeatures()
        {
            _featureDimensions[0] = new FeatureDimension("LengthOfPath");
            _featureDimensions[1] = new FeatureDimension("NumberOfCorners");
            _featureDimensions[2] = new FeatureDimension("NumberOfJumps");
        }


        public override List<Chromosome> Run(Func<Chromosome, double> fitness)
        {
            Stopwatch GATimer = new Stopwatch();
            
            GATimer.Start();
            int G = 10;

            for (int i = 0; i < _iteration; i++)
            {
                Chromosome one;
                // Chromosome two;
                if (i < G)
                {
                    List<Chromosome> parents = _operators.TournamentSelection(currentPopulation);
                    List<Chromosome> tmp = _operators.SinglePointCrossover(parents[0], parents[1]);
                    one = tmp[0];
                    // two = tmp[1];
                }
                else
                {
                    one = currentPopulation[_random.Next()];
                    one = _operators.UniformMutation(one, _mutationProbability, GenerateGene);
                }

                PlaceInMapElites(one, fitness);
            }
            
            GATimer.Stop();

            return new List<Chromosome>(){GetMostPromisingSolution()};
        }

        public override void SaveInfo()
        {
            throw new NotImplementedException();
        }

        private Chromosome GetMostPromisingSolution()
        {
            float bestPerformance = mapOfElites_fitness.Cast<float>().Max();
            
            int bestPerformanceIndex = mapOfElites_fitness.Cast<float>().ToList().IndexOf(bestPerformance);  
            
            Chromosome chromo = mapOfElites.Cast<Chromosome>().ToArray()[bestPerformanceIndex];

            return chromo;

        }
        
        // fitness == to performance
        public void PlaceInMapElites(Chromosome one, Func<Chromosome, double> fitness)
        {
            double _fitness = fitness(one);
            // Logger.Log(Logger.LogTarget.BasicGeneticOutput,
            //     $"{currentPopulation[i].id_},{time},{currentPopulation[i].fitness},{iter}");
            int[] getIndices = GetFeatureValue(one);

            double currentFitnessInCell = mapOfElites_fitness[getIndices[0], getIndices[1], getIndices[2]];
            if (currentFitnessInCell == 0.0f || currentFitnessInCell < _fitness)
            {
                mapOfElites_fitness[getIndices[0], getIndices[1], getIndices[2]] = _fitness;
                mapOfElites[getIndices[0], getIndices[1], getIndices[2]] = one;
            }
        }

        private int[] GetFeatureValue(Chromosome chromosome)
        {
            int[] featureValues = new int[N];
            
            for (int i =0; i < N; i++)
            {
                float tmp = _featureDimensions[i].Calculate(chromosome);
                featureValues[i] = _featureDimensions[i].Discretize(tmp);
            }

            return featureValues;
        }
    }

}
