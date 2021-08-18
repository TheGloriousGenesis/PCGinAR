using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeneticAlgorithms.Entities;
using UnityEngine;
using Utilities;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace GeneticAlgorithms.Algorithms
{
    [Serializable]
    public class BasicGeneticAlgorithm : BaseAlgorithm
    {
        public List<Chromosome> _currentPopulation;
        private List<Chromosome> _runPopulation;
        private int CurrentNumberOfRuns;
        public IEnumerable<Chromosome> _currentElite;
        private Stopwatch stop = new Stopwatch();

        public BasicGeneticAlgorithm(int populationSize, int chromosomeLength, float crossoverProbability,
            Random randomG,
            int elitism, float mutationProbability, int maxNumberOfMutations, int iteration, int k, int x, int y, int z,
            Func<Chromosome, FitnessValues> fitnessFunction) :
            base(populationSize, chromosomeLength, crossoverProbability, randomG, elitism,
                mutationProbability, maxNumberOfMutations, iteration, k, x, y, z, fitnessFunction)
        {
            _currentPopulation = GenerateGenotype(populationSize);
            _runPopulation = new List<Chromosome>(populationSize);
            _currentElite = new List<Chromosome>(populationSize);
        }

        public override List<Chromosome> Run()
        {
            CurrentNumberOfRuns++;
            totalTime = 0;
            timer.Reset();
            timer.Start();
            
            stop.Reset();
            stop.Start();
            // generate new individuals with new weights. Keep top performing of previous run
            List<Chromosome> tmp = new List<Chromosome>();
            tmp.AddRange(_currentElite);
            while (tmp.Count < _currentPopulation.Count)
            {
                tmp.Add(GenerateChromosome());
            }
            _currentPopulation = tmp;
            stop.Stop();
            Debug.Log($"Set up population takes: {stop.Elapsed.TotalMilliseconds} ms");

            float sum = 0.0f;

            for (int iter = 0; iter < Iteration; ++iter)
            {
                _runPopulation = new List<Chromosome>();

                int elites = Elitism;

                stop.Reset();
                stop.Start();
                // calculate fitness for test population.
                sum = 0.0f;
                foreach (var t in _currentPopulation)
                {
                    FitnessValues fv = FitnessFunction(t);

                    AddDataToResults($"{iter},{t.ID},{fv.time},{fv.fitness},{fv.linearity},{fv.pathLength}," +
                                     $"{fv.numberOfPaths},{fv.nullSpace},{fv.walkableSurface}");
                    sum += fv.fitness;
                }
                stop.Stop();
                Debug.Log($"Population fitness timing: {stop.Elapsed.TotalMilliseconds} ms");
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
                    }
                }
                (_currentPopulation, _runPopulation) = (_runPopulation, _currentPopulation);
            }
            timer.Stop();
            totalTime = timer.ElapsedMilliseconds;
            // Debug.Log($"Total time (ms): {totalTime}");
            Debug.Log($"Platform Height: {PlatformHeight}");
            Debug.Log($"Platform Width: {PlatformWidth}");
            Debug.Log($"Elitsm: {Elitism}");
            AddDataToResults_Time($"{CurrentNumberOfRuns},{totalTime},{PlatformHeight},{PlatformWidth}");
            
            OutputTestResults(LogTarget.BasicGeneticOutput);
            OutputTestResults_Time(LogTarget.Time);
            OutputTestResults_Weights(LogTarget.WeightedChunks);
            
            // find best-fitting chromosomes
            _currentPopulation.Sort(Utility.CompareChromosome);
            _currentElite = _currentPopulation.Take(Elitism);
            return _currentElite.ToList();
        }
    }
}