using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BaseGeneticClass;

namespace BasicGeneticAlgorithmNS
{
    // TODO: Check that a given chromosone has unique genes that do not overlap in space (maybe use hashset)
    [System.Serializable]
    public class BasicGeneticAlgorithm
    {
        private int _populationSize;
        private int _iteration; // same as number of generations
        private int _chromosoneSize;
        private Func<Chromosome, double> _fitnessFunction;
        private float _crossoverProbability;
        private float _mutationProbability;
        System.Random _random;
        private int _elitism;
        private int _k;
        private List<Chromosome> currentPopulation;
        private List<Chromosome> runPopulation;


        public BasicGeneticAlgorithm(int populationSize, int chromosoneLength, float crossoverProbability, System.Random random,
            Func<Chromosome, double> fitnessFunction, int elitism, float mutationProbability, int iteration, int k)
        {
            this._populationSize = populationSize;
            this._chromosoneSize = chromosoneLength;
            this._crossoverProbability = crossoverProbability;
            this._random = random;
            this._fitnessFunction = fitnessFunction;
            this._elitism = elitism;
            this._mutationProbability = mutationProbability;
            this._iteration = iteration;
            this._fitnessFunction = fitnessFunction;
            this._k = k;
            // create initial population
            this.currentPopulation = GenerateGenotype(populationSize);
            this.runPopulation = new List<Chromosome>(populationSize);
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
        
        #region Genetic Operators
        
        // Tournament Selection
        public List<Chromosome> Select(List<Chromosome> population)
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

        // Single Point Crossover 
        public List<Chromosome> Crossover(Chromosome chromosome1, Chromosome chromosome2)
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
        
        // Random Resetting/ Uniform Mutation
        public Chromosome Mutate(Chromosome chromosome, double probability)
        {
            // replaces a gene with random new gene.
            if (_random.NextDouble() < probability)
            {
                List<Gene> genes = chromosome.genes;
                int rv = _random.Next(0, chromosome.genes.Count);
                
                genes[rv] = GenerateGene();
                chromosome.genes = genes;
            }
            return chromosome;
        }

        #endregion
        
        #region Implementation
        public List<Chromosome> Run(Func<Chromosome, double> fitness)
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
                    HandleTextFile.WriteToFile("GenerationResults",
                        $"{currentPopulation[i].id_},{time},{currentPopulation[i].fitness},{iter}");
                }
                
                // for last iteration, population does not need to be generated
                if (iter == _iteration - 1) break;

                while (runPopulation.Count < currentPopulation.Count)
                {
                    parents = Select(currentPopulation);
                    one = parents[0];
                    two = parents[1];

                    // determine if crossover occurs.
                    randDouble = _random.NextDouble();
                    if (randDouble <= _crossoverProbability)
                    {
                        List<Chromosome> chromosomes = Crossover(one, two);
                        
                        one = chromosomes[0];
                        two = chromosomes[1];
                    }

                    one = Mutate(one, _mutationProbability);
                    two = Mutate(two, _mutationProbability);

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
        #endregion
    }

}
