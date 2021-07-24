using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GeneticAlgorithms;
using BaseGeneticClass;
using System.Linq;

namespace Tests
{
    public class BasicGeneticAlgorithmTest
    {
        private BasicGeneticAlgorithm test;

        private int testPopSize = 5;
        private int testChromosomeLen = 6;
        private int testCrossoverProb = 6;
        private float testMutationProb = 0.2f;
        private System.Random testSeed = new System.Random(123);
        private int testElitism = 3;
        private int testIteration = 3;
        private int testK = 2;
        private Operators _operators;

        [SetUp]
        public void SetUp()
        {
            _operators = new Operators(testSeed, testK);
            test = new BasicGeneticAlgorithm(
                testPopSize, testChromosomeLen, testCrossoverProb,
                testSeed, testElitism, testMutationProb, testIteration, testK);
        }

        [Test]
        public void TestSelectOperator()
        {
            Chromosome pop1 = new Chromosome {genes = new List<Gene>(), fitness = 0.67f};

            Chromosome pop2 = new Chromosome {genes = new List<Gene>(), fitness = 0.34f};

            Chromosome pop3 = new Chromosome {genes = new List<Gene>(), fitness = 0.94f};

            Chromosome pop4 = new Chromosome {genes = new List<Gene>(), fitness = 0.99f};

            List<Chromosome> population = new List<Chromosome> {pop1, pop2, pop3, pop4};

            List<Chromosome> result = _operators.TournamentSelection(population);

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
            Chromosome pop1 = new Chromosome {genes = new List<Gene>(), fitness = 0.67f};

            Chromosome pop2 = new Chromosome {genes = new List<Gene>(), fitness = 0.34f};

            Chromosome pop3 = new Chromosome {genes = new List<Gene>(), fitness = 0.94f};

            Chromosome pop4 = new Chromosome {genes = new List<Gene>(), fitness = 0.99f};
            
            List<Chromosome> population = new List<Chromosome>() {pop1, pop2, pop3, pop4};

            List<Chromosome> result = Utility.FindNBestFitness_ByChromosome(population, 1);

            Assert.AreEqual(1, result.Count);
            
            Assert.AreEqual(result[0].fitness, pop4.fitness);
        }
        
    }
}
