using System;
using System.Collections.Generic;
using GeneticAlgorithms.Algorithms;
using GeneticAlgorithms.Entities;
using NUnit.Framework;
using Utilities;

namespace Tests
{
    public class BasicGeneticAlgorithmTest
    {
        private BasicGeneticAlgorithm test;

        private int testPopSize = 5;
        private int testChromosomeLen = 6;
        private int testCrossoverProb = 6;
        private float testMutationProb = 0.2f;
        private Random testSeed = new Random(123);
        private int testElitism = 3;
        private int testIteration = 3;
        private int testK = 2;
        private int testX = 2;
        private int testY = 2;
        private int testZ = 2;
        private GeneticOperators _geneticOperators;
        private Func<Chromosome, FitnessValues> fitnessFunction;

        [SetUp]
        public void SetUp()
        {
            _geneticOperators = new GeneticOperators(testSeed, testK);
        //     test = new BasicGeneticAlgorithm(
        //         testPopSize, testChromosomeLen, testCrossoverProb,
        //         testSeed, testElitism, testMutationProb, testIteration, testK, testX, testY, testZ, fitnessFunction);
        }

        [Test]
        public void TestSelectOperator()
        {
            Chromosome pop1 = new Chromosome {Genes = new List<Gene>(), Fitness = 0.67f};

            Chromosome pop2 = new Chromosome {Genes = new List<Gene>(), Fitness = 0.34f};

            Chromosome pop3 = new Chromosome {Genes = new List<Gene>(), Fitness = 0.94f};

            Chromosome pop4 = new Chromosome {Genes = new List<Gene>(), Fitness = 0.99f};

            List<Chromosome> population = new List<Chromosome> {pop1, pop2, pop3, pop4};

            List<Chromosome> result = _geneticOperators.TournamentSelection(population);

            Assert.AreEqual(2, result.Count);
        }
        
        [Test]
        public void TestCrossoverOperator()
        {
            
        }

        [Test]
        public void TestMutateOperator()
        {
        }


        private double testFitnessFunction(Chromosome chromosome)
        {
            return 23d;
        }
    }

    public class UtilityTest
    {
        [Test]
        public void TestFindNBestFitness_ByChromosome()
        {
            Chromosome pop1 = new Chromosome {Genes = new List<Gene>(), Fitness = 0.67f};

            Chromosome pop2 = new Chromosome {Genes = new List<Gene>(), Fitness = 0.34f};

            Chromosome pop3 = new Chromosome {Genes = new List<Gene>(), Fitness = 0.94f};

            Chromosome pop4 = new Chromosome {Genes = new List<Gene>(), Fitness = 0.99f};
            
            List<Chromosome> population = new List<Chromosome>() {pop1, pop2, pop3, pop4};

            List<Chromosome> result = Utility.FindNBestFitness_ByChromosome(population, 1);

            Assert.AreEqual(1, result.Count);
            
            Assert.AreEqual(result[0].Fitness, pop4.Fitness);
        }
        
    }
}
