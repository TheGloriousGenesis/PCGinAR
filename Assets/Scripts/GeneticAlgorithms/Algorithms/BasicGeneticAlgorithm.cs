using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Entities;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;
using Random = System.Random;

namespace GeneticAlgorithms.Algorithms
{
    [Serializable]
    public class BasicGeneticAlgorithm : BaseAlgorithm
    {
        public List<Chromosome> _currentPopulation;
        private List<Chromosome> _runPopulation;

        public BasicGeneticAlgorithm(int populationSize, int chromosomeLength, float crossoverProbability,
            Random randomG,
            int elitism, float mutationProbability, int maxNumberOfMutations, int iteration, int k, int x, int y, int z,
            Func<Chromosome, FitnessValues> fitnessFunction) :
            base(populationSize, chromosomeLength, crossoverProbability, randomG, elitism,
                mutationProbability, maxNumberOfMutations, iteration, k, x, y, z, fitnessFunction)
        {
            _currentPopulation = GenerateGenotype(populationSize);
            _runPopulation = new List<Chromosome>(populationSize);
        }

        public override List<Chromosome> Run()
        {
            totalTime = 0;
            timer.Start();
            float sum = 0.0f;

            for (int iter = 0; iter < Iteration; ++iter)
            {
                totalTime += Time.deltaTime;

                _runPopulation = new List<Chromosome>();

                int elites = Elitism;

                // calculate fitness for test population.
                sum = 0.0f;
                foreach (var t in _currentPopulation)
                {
                    FitnessValues fv = FitnessFunction(t);

                    AddDataToResults($"{iter},{t.ID},{fv.time},{fv.fitness},{fv.linearity},{fv.pathLength}," +
                                     $"{fv.numberOfPaths},{fv.nullSpace},{fv.walkableSurface}");
                    sum += fv.fitness;
                }

                _currentPopulation.Sort(Utility.CompareChromosome);

                // for last iteration, population does not need to be generated
                if (iter == Iteration - 1) break;

                while (_runPopulation.Count < _currentPopulation.Count)
                {
                    // Survivor selection
                    if (elites > 0)
                    {
                        _runPopulation.Add(GeneticGeneticOperator.FitnessProportionateSelection
                            (_currentPopulation, sum));
                        elites--;
                    }
                    else
                    {
                        var parents = GeneticGeneticOperator.TournamentSelection(_currentPopulation);
                        var one = parents[0];
                        var two = parents[1];

                        // determine if crossover occurs.
                        var randDouble = RandomG.NextDouble();
                        if (randDouble <= CrossoverProbability)
                        {
                            List<Chromosome> chromosomes = GeneticGeneticOperator.SinglePointCrossover(one, two);

                            one = chromosomes[0];
                            two = chromosomes[1];
                        }

                        one = GeneticGeneticOperator.UniformMutation(one, MutationProbability, GenerateGene, MaxNumberOfMutations);
                        two = GeneticGeneticOperator.UniformMutation(two, MutationProbability, GenerateGene, MaxNumberOfMutations);

                        _runPopulation.Add(one);
                        _runPopulation.Add(two);
                        double fit1 = FitnessFunction(one).fitness;
                        double fit2 = FitnessFunction(two).fitness;

                        if (fit1 > fit2)
                        {
                            if (fit1 > parents[0].Fitness || fit1 > parents[1].Fitness)
                            {
                                one.betterThanParent = true;
                                _runPopulation.Add(one);
                            }
                        }
                        // pick which offspring is sent into the next population
                        else if (fit2 > fit1)
                        {
                            if (fit2 > parents[0].Fitness || fit2 > parents[1].Fitness)
                            {
                                two.betterThanParent = true;
                                _runPopulation.Add(two);
                            }
                        }
                        else
                        {
                            _runPopulation.Add(RandomG.Next(0, 1) == 1 ? parents[1] : parents[0]);
                        }
                    }
                }

                (_currentPopulation, _runPopulation) = (_runPopulation, _currentPopulation);
            }

            timer.Stop();
            totalTime = timer.ElapsedMilliseconds;
            Debug.Log($"Total time (ms): {totalTime}");
            OutputTestResults(LogTarget.BasicGeneticOutput);
            // find best-fitting chromosomes
            _currentPopulation.Sort(Utility.CompareChromosome);
            return _currentPopulation.Take(5).ToList();
        }

        private void RemoveLessFitFromPopulation(List<Chromosome> population)
        {
            population.Sort(Utility.CompareChromosome);

            population.Remove(population[population.Count - 1]);
        }
    }
}