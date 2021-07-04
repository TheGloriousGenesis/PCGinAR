using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BasicGeneticAlgorithmNS;
using BaseGeneticClass;
using System.Linq;

namespace Tests
{
    public class BasicGeneticAlgorithmTest
    {
        private BasicGeneticAlgorithm test;

        [SetUp]
        public void SetUp()
        {
            test = new BasicGeneticAlgorithm(Constants.POPULATION_SIZE, Constants.CHROMOSONE_LENGTH, Constants.CROSSOVER_PROBABILITY,
                new System.Random(Constants.SEED), testFitnessFunction, Constants.ELITISM, Constants.MUTATION_PROBABILITY, Constants.ITERATION);
        }

        [Test]
        public void GenerateGenotype_Test()
        {
            //List<Chromosone> result = test.GenerateGenotype();
            //Assert.AreEqual(Constants.POPULATION_SIZE, result.Count);

            //List<Gene> result_gene = result[0].genes;
            //Assert.AreEqual(Constants.CHROMOSONE_LENGTH, result_gene.Count);
        }

        [Test]
        public void GenerateGene_Test()
        {
            Allele result = test.GenerateGene().allele;
            Assert.AreEqual(2, result.blockPositions.Count);

            Vector3 centerBlock = result.blockPositions[0];

            Assert.LessOrEqual(centerBlock.x, Constants.MAX_PLATFORM_DIMENSION);
            Assert.LessOrEqual(centerBlock.y, Constants.MAX_PLATFORM_DIMENSION);
            Assert.LessOrEqual(centerBlock.z, Constants.MAX_PLATFORM_DIMENSION);
        }

        [Test]
        public void Select_ShouldSelectParentsCorrectly()
        {
            List<Chromosone> population = new List<Chromosone>();
            double[] fitnesses = new double[4];

            Chromosone pop1 = new Chromosone();
            pop1.genes = new List<Gene>();

            Chromosone pop2 = new Chromosone();
            pop2.genes = new List<Gene>();

            Chromosone pop3 = new Chromosone();
            pop3.genes = new List<Gene>();

            Chromosone pop4 = new Chromosone();
            pop4.genes = new List<Gene>();

            population.Add(pop1);
            population.Add(pop2);
            population.Add(pop3);
            population.Add(pop4);

            fitnesses[0] = 0.67;
            fitnesses[1] = 0.34;
            fitnesses[2] = 0.94;
            fitnesses[3] = 0.99;

            List<Chromosone> result = test.Select(population, fitnesses);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains(pop3), "result does not contain poulation 3");
            Assert.IsTrue(result.Contains(pop4), "result does not contain poulation 4");
        }

        [Test]
        public void Select_ShouldSelectNoParentIfNotEnoughInPopulation()
        {
            List<Chromosone> population = new List<Chromosone>();
            double[] fitnesses = new double[4];

            List<Chromosone> result = test.Select(population, fitnesses);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Mutate_shouldMutateChromosoneCorrectly()
        {
            //List<Gene> testGenes = new List<Gene>();
            //Allele allele_1 = new Allele(new Vector3(3, 4, 2), new List<Vector3> { BlockPosition.LEFT, BlockPosition.FRONT });
            //Gene g1 = new Gene(allele_1);
            //testGenes.Add(g1);

            //Allele allele_2 = new Allele(new Vector3(6, 7, 8), new List<Vector3> { BlockPosition.LEFT, BlockPosition.BACK });
            //Gene g2 = new Gene(allele_2);
            //testGenes.Add(g2);

            //Chromosone chromosone = new Chromosone(testGenes);

            //Chromosone copy = chromosone.DeepCopy();

            //Chromosone result = test.Mutate(chromosone, 1.0d);
            //Assert.IsFalse(result.genes.Equals(copy.genes), "Lists are equal");
        }

        [Test]
        public void Crossover_ShouldCrossoverChromosones()
        {
            //List<Gene> testGenes = new List<Gene>();
            //Allele allele_1 = new Allele(new Vector3(3, 4, 2), new List<Vector3> { BlockPosition.UP, BlockPosition.DOWN });
            //Gene g1 = new Gene(allele_1);
            //testGenes.Add(g1);

            //Allele allele_2 = new Allele(new Vector3(6, 7, 8), new List<Vector3> { BlockPosition.UP, BlockPosition.DOWN });
            //Gene g2 = new Gene(allele_2);
            //testGenes.Add(g2);

            //Chromosone chromosone = new Chromosone(testGenes);

            //Chromosone copy = chromosone.DeepCopy();

            //Chromosone result = test.Mutate(chromosone, 1.0d);
            //Assert.IsFalse(result.genes.Equals(copy.genes), "Lists are equal");
        }

        private float testFitnessFunction(Chromosone chromosone)
        {
            return chromosone.fitness;
        }
    }
}
