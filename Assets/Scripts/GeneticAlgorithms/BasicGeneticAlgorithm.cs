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
        //private Chromosone solution;
        public double Fitness(Chromosone chromosome)
        {
            float platformValue = chromosome.CalculateFitness();
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
        //private static int Compute(Chromosone s)
        //{
        //    // to compute fitness implement agent to see if it can reach end point
        //    // or implement a*
        //    //int n = s.genes.Count;
        //    //int m = t.Length;
        //    //var d = new int[n + 1, m + 1];

        //    //// Step 1
        //    //if (n == 0)
        //    //{
        //    //    return m;
        //    //}

        //    //if (m == 0)
        //    //{
        //    //    return n;
        //    //}

        //    //// Step 2
        //    //for (int i = 0; i <= n; ++i)
        //    //{
        //    //    d[i, 0] = i;
        //    //}
        //    //for (int j = 0; j <= m; ++j)
        //    //{
        //    //    d[j, 0] = j;
        //    //}

        //    //// Step 3
        //    //for (int i = 1; i <= n; i++)
        //    //{
        //    //    //Step 4
        //    //    for (int j = 1; j <= m; j++)
        //    //    {
        //    //        // Step 5
        //    //        int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

        //    //        // Step 6
        //    //        d[i, j] = Math.Min(
        //    //            Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
        //    //            d[i - 1, j - 1] + cost
        //    //        );
        //    //    }
        //    //}
        //    //// Step 7
        //    //return d[n, m];
        //    return 0;
        //}
    }


    // TODO: Check that a given chromosone has unique genes that do not overlap in space (maybe use hashset)
    [System.Serializable]
    public class BasicGeneticAlgorithm
    {
        private int populationSize;
        private int iteration;
        private int chromosoneSize;
        private Func<Chromosone, float> fitnessFunction;
        private float crossoverProbability;
        private float mutationProbability;
        System.Random random;
        private int elitism;
        private List<Chromosone> testPopulation { get; set; }

        public BasicGeneticAlgorithm(int pOPULATION_SIZE, int cHROMOSONE_LENGTH, float cROSSOVER_PROBABILITY, System.Random random,
            Func<Chromosone, float> fitnessFunction, int eLITISM, float mUTATION_PROBABILITY, int iTERATION)
        {
            this.populationSize = pOPULATION_SIZE;
            this.chromosoneSize = cHROMOSONE_LENGTH;
            this.crossoverProbability = cROSSOVER_PROBABILITY;
            this.random = random;
            this.fitnessFunction = fitnessFunction;
            this.elitism = eLITISM;
            this.mutationProbability = mUTATION_PROBABILITY;
            this.iteration = iTERATION;
            this.fitnessFunction = fitnessFunction;
            this.testPopulation = GenerateGenotype(pOPULATION_SIZE);
        }

        public List<Chromosone> GenerateGenotype(int popSize)
        {
            // A Genotype is the population in computation space
            List<Chromosone> genotype = new List<Chromosone>();
            for (int i=0; i < popSize; i++)
            {
                genotype.Add(GenerateChromosome(i));
            }

            return genotype;
        }

        public Chromosone GenerateChromosome(int id)
        {
            // a genotype is a solution to the level. feed in number of blocks and restrictions to generate possible level
            List<Gene> genes = new List<Gene>();

            // todo: check how many genes in chromosone good fit
            for (int i=0; i < chromosoneSize; i++)
            {
                genes.Add(GenerateGene());
            }

            Chromosone chromosone = new Chromosone();
            chromosone.id_ = id;
            chromosone.genes = genes;
            return chromosone;
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
                int choosePosition = random.Next(0, cubeNumbers.Count);
                positions.Add(cubeNumbers[choosePosition] + centerBlock);
                cubeNumbers.Remove(cubeNumbers[choosePosition]);
            }
            
            return new Allele(positions);
        }

        private Vector3 GenerateCenterBlockPosition()
        {
            // good for condensing the size of the space
            int maxPlatformLength = Constants.MAX_PLATFORM_DIMENSION / 2;
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
                genes[rv] = GenerateGene();
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
        public List<Chromosone> Run(Func<Chromosone, float> fitness)
        {
            //List<Chromosone> testPopulation = GenerateGenotype();
            List<Chromosone> runPopulation = new List<Chromosone>();

            double[] fitnesses = new double[populationSize];

            double sum = 0.0;

            Chromosone one;
            Chromosone two;
            List<Chromosone> parents;

            double randDouble = 0.0;

            for (int iter = 0; iter < iteration; iter++)
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

                Debug.Log($"Sum of fitness at ITERATION - {iter + 1} : {sum}");

                if (iter == iteration - 1) break;

                while (runPopulation.Count < testPopulation.Count)
                {
                    parents = Select(testPopulation, fitnesses);
                    one = parents[0];
                    two = parents[1];

                    // determine if crossover occurs.
                    randDouble = Random.value;
                    if (randDouble <= crossoverProbability)
                    {
                        List<Chromosone> chromosones = Crossover(one, two).ToList();
                        one = chromosones[0];
                        two = chromosones[1];
                    }

                    one = Mutate(one, mutationProbability);
                    two = Mutate(two, mutationProbability);

                    runPopulation.Add(one);
                    runPopulation.Add(two);
                }

                testPopulation = runPopulation;
            }

            // find best-fittin chromosones
            Chromosone[] testSort = testPopulation.ToArray();
            double[] fitSort = fitnesses.ToArray();

            Array.Sort(fitSort, testSort);
            Debug.Log("Array length: " + testSort.Length);
            return testSort.Take(1).ToList();
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
