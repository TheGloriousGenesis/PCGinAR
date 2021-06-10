using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using BaseGeneticClass;

namespace BasicGeneticAlgorithmNS
{
    [System.Serializable]
    public class FitnessHelper
    {
        // assumes chromosome and solution are bitstrings
        private Chromosone solution;
        public double Fitness(Chromosone chromosome)
        {
            int platformValue = Compute(chromosome);
            // normalise value to be between 0 and 1
            double score = 1.0 / (double)(platformValue + 1);

            return score;
        }

        //public FitnessHelper(Chromosone targetSolution)
        //{
        //    solution = targetSolution;
        //}

        // https://en.wikipedia.org/wiki/Levenshtein_distance
        // https://www.dotnetperls.com/levenshtein
        // dynamic programming
        private static int Compute(Chromosone s)
        {
            // to compute fitness implement agent to see if it can reach end point
            // or implement a*
            //int n = s.genes.Count;
            //int m = t.Length;
            //var d = new int[n + 1, m + 1];

            //// Step 1
            //if (n == 0)
            //{
            //    return m;
            //}

            //if (m == 0)
            //{
            //    return n;
            //}

            //// Step 2
            //for (int i = 0; i <= n; ++i)
            //{
            //    d[i, 0] = i;
            //}
            //for (int j = 0; j <= m; ++j)
            //{
            //    d[j, 0] = j;
            //}

            //// Step 3
            //for (int i = 1; i <= n; i++)
            //{
            //    //Step 4
            //    for (int j = 1; j <= m; j++)
            //    {
            //        // Step 5
            //        int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

            //        // Step 6
            //        d[i, j] = Math.Min(
            //            Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
            //            d[i - 1, j - 1] + cost
            //        );
            //    }
            //}
            //// Step 7
            //return d[n, m];
            return 0;
        }
    }


    // TODO: Check that a given chromosone has unique genes that do not overlap in space (maybe use hashset)
    [System.Serializable]
    public class BasicGeneticAlgorithm
    {
        public bool[,,] blockMatrix = new bool[Constants.MAX_PLATFORM_LENGTH, Constants.MAX_PLATFORM_LENGTH, Constants.MAX_PLATFORM_LENGTH];

        public List<Chromosone> GenerateGenotype(int maxPlatformLength, int populationSize)
        {
            // A Genotype is the population in computation space
            List<Chromosone> genotypes = new List<Chromosone>();
            for (int i=0; i < populationSize; i++)
            {
                genotypes.Add(GenerateChromosome(maxPlatformLength));
            }

            return genotypes;
        }

        public Chromosone GenerateChromosome(int maxPlatformLength)
        {
            // a genotype is a solution to the level. feed in number of blocks and restrictions to generate possible level
            List<Gene> genes = new List<Gene>();
            
            for (int i=0; i < 10; i++)
            {
                genes.Add(GenerateGene(maxPlatformLength));
            }

            Chromosone chromosone = new Chromosone();
            chromosone.genes = genes;
            return chromosone;
        }

        public Gene GenerateGene(int maxPlatformLength)
        {
            return new Gene(GenerateAllele(maxPlatformLength));
        }

        // Generates a 3 cubed structure as a single gene
        public Allele GenerateAllele(int maxPlatformLength)
        {
            // inclusive 
            List<int> cubeNumbers = Enumerable.Range(0, 5).ToList();

            Vector3 centerBlock = GenerateCenterBlockPosition(maxPlatformLength);

            List<Vector3> positions = new List<Vector3>();

            for (int i = 0; i < 2; i++)
            {
                // inclusive
                int choosePosition = Random.Range(0, cubeNumbers.Count - 1);
                positions.Add(GenerateRelativePosition(choosePosition));
                cubeNumbers.Remove(choosePosition);
            }

            
            return new Allele(centerBlock, positions);
        }

        private Vector3 GenerateCenterBlockPosition(int maxPlatformLength)
        {
            int xPos = Random.Range(0, maxPlatformLength);
            int yPos = Random.Range(0, maxPlatformLength);
            int zPos = Random.Range(0, maxPlatformLength);
            return new Vector3(xPos, yPos, zPos);
        }

        private Vector3 GenerateRelativePosition(int choosePosition)
        {
            switch (choosePosition)
            {
                case 0:
                    return BlockPosition.UP;
                case 1:
                    return BlockPosition.DOWN;
                case 2:
                    return BlockPosition.LEFT;
                case 3:
                    return BlockPosition.RIGHT;
                case 4:
                    return BlockPosition.FRONT;
                case 5:
                    return BlockPosition.BACK;
                default:
                    return BlockPosition.NONE;
            }
        }

        private int[] GetRandomElements(int listLength, int elementsCount)
        {
            return Enumerable.Range(0, listLength).OrderBy(x => Guid.NewGuid()).Take(elementsCount).ToArray();
        }
        public int[] FindKBiggestNumbersInArray(double[] arr, int k)
        {
            var indexes =  arr.Select((val, idx) => (val: val, idx: idx))
                             .OrderByDescending(p => p.val)
                             .Take(k)
                             .Select(p => p.idx);
            return indexes.ToArray();
        }

        // Does this need to be unique? or can the select be a duplicate
        public List<Chromosone> Select(List<Chromosone> population, double[] fitnesses)
        {
            // Tournamet selection select best k individuals from the population at random and select the best of out these
            // to become a parent. Same process repeated for selecting the next parent.
            List<Chromosone> selections = new List<Chromosone>();
            if (population.Count() < 4)
            {
                return selections;
            }

            List<double> fitList = fitnesses.ToList();

            // pick 4 random indices. Then pick out the fitnesses in relation to these indices and
            // pick the top 4 ENSURE POPULATION HAS MINIMUM 4 ELEMENTS
            int[] pickRandomIndices = GetRandomElements(fitList.Count, 4);

            double[] fitnessSelection = pickRandomIndices.Select(x => fitList[x]).ToArray();

            int[] topIndices = FindKBiggestNumbersInArray(fitnessSelection, 2);

            selections.Add(population[pickRandomIndices[topIndices[0]]]);
            selections.Add(population[pickRandomIndices[topIndices[1]]]);

            return selections;
        }

        public Chromosone Mutate(Chromosone chromosome, double probability)
        {
            // uniform mutation
            // replaces a gene with random new gene.
            if (Random.value < probability)
            {
                List<Gene> genes = chromosome.genes;
                int rv = Random.Range(0, chromosome.genes.Count - 1);
                genes[rv] = GenerateGene(Constants.MAX_PLATFORM_LENGTH);
                chromosome.genes = genes;
            }
            return chromosome;
        }

        public List<Chromosone> Crossover(Chromosone chromosome1, Chromosone chromosome2)
        {
            // Single Point Crossover 
            int randomPosition = Random.Range(0, chromosome1.genes.Count);
            List<Gene> newChromosome1 = chromosome1.genes.GetRange(0, randomPosition)
                .Concat(chromosome2.genes.GetRange(randomPosition, chromosome2.genes.Count())).ToList();
            List<Gene> newChromosome2 = chromosome2.genes.GetRange(0, randomPosition)
                .Concat(chromosome1.genes.GetRange(randomPosition, chromosome1.genes.Count())).ToList();

            Chromosone chomesome1 = new Chromosone();
            chomesome1.genes = newChromosome1;
            Chromosone chomesome2 = new Chromosone();
            chomesome2.genes = newChromosome2;

            return new List<Chromosone> { chomesome1, chomesome2 };
        }
        
        //input chromosone output double for Func
        public List<Chromosone> Run(Func<Chromosone, double> fitness)
        {
            List<Chromosone> testPopulation = GenerateGenotype(Constants.MAX_PLATFORM_LENGTH, Constants.POPULATION_SIZE);
            List<Chromosone> runPopulation = new List<Chromosone>();

            double[] fitnesses = new double[Constants.POPULATION_SIZE];

            double sum = 0.0;

            Chromosone one;
            Chromosone two;
            List<Chromosone> parents;

            double randDouble = 0.0;

            for (int iter = 0; iter < Constants.ITERATION; iter++)
            {
                runPopulation = new List<Chromosone>();

                // calculate fitness for test population.
                sum = 0.0;
                fitnesses = new double[testPopulation.Count];
                for (int i = 0; i < fitnesses.Length; ++i)
                {
                    fitnesses[i] = fitness(testPopulation[i]);
                    sum += fitnesses[i];
                }

                if (iter == Constants.ITERATION - 1) break;

                while (runPopulation.Count < testPopulation.Count)
                {
                    parents = Select(testPopulation, fitnesses);
                    one = parents[0];
                    two = parents[1];

                    // determine if crossover occurs.
                    randDouble = Random.value;
                    if (randDouble <= Constants.CROSSOVER_PROBABILITY)
                    {
                        List<Chromosone> chromosones = Crossover(one, two).ToList();
                        one = chromosones[0];
                        two = chromosones[1];
                    }

                    one = Mutate(one, Constants.MUTATION_PROBABILITY);
                    two = Mutate(two, Constants.MUTATION_PROBABILITY);

                    runPopulation.Add(one);
                    runPopulation.Add(two);
                }

                testPopulation = runPopulation;
            }

            // find best-fittin chromosones
            Chromosone[] testSort = testPopulation.ToArray();
            double[] fitSort = fitnesses.ToArray();

            Array.Sort(fitSort, testSort);

            return (List<Chromosone>)testSort.Take(1);
        }

        public int GetRandomWeightedIndex(float[] weights)
        {
            if (weights == null || weights.Length == 0) return -1;

            float w;
            float t = 0;
            int i;
            for (i = 0; i < weights.Length; i++)
            {
                w = weights[i];

                if (float.IsPositiveInfinity(w))
                {
                    return i;
                }
                else if (w >= 0f && !float.IsNaN(w))
                {
                    t += weights[i];
                }
            }

            float r = Random.value;
            float s = 0f;

            for (i = 0; i < weights.Length; i++)
            {
                w = weights[i];
                if (float.IsNaN(w) || w <= 0f) continue;

                s += w / t;
                if (s >= r) return i;
            }

            return -1;
        }
    }

}
