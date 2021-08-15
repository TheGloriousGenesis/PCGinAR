using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Parameter;
using UnityEngine.Serialization;
using Utilities;

namespace GeneticAlgorithms.Algorithms
{
    [Serializable]
    public class BasicGeneticAlgorithm : BaseAlgorithm
    {
        public List<Chromosome> _currentPopulation;
        private List<Chromosome> _runPopulation;
        [FormerlySerializedAs("playedLevels")] 
        public List<Chromosome> goodPlayedLevels;
        public List<Chromosome> badPlayedLevels;


        public BasicGeneticAlgorithm(int populationSize, int chromosomeLength, float crossoverProbability, Random randomG, 
            int elitism, float mutationProbability, int iteration, int k, int x, int y, int z) : 
            base(populationSize, chromosomeLength, crossoverProbability, randomG, elitism, 
                mutationProbability, iteration, k, x, y, z)
        {
            _currentPopulation = GenerateGenotype(populationSize);
            _runPopulation = new List<Chromosome>(populationSize);
            goodPlayedLevels = new List<Chromosome>();
            badPlayedLevels = new List<Chromosome>();
        }

        public override List<Chromosome> Run(Func<Chromosome, FitnessValues> fitness)
        {
            float sum = 0.0f;

            for(int iter = 0; iter < Iteration; ++iter) {
                    _runPopulation = new List<Chromosome>();

                    int elites = Elitism;

                    // calculate fitness for test population.
                    sum = 0.0f;
                    foreach (var t in _currentPopulation)
                    {
                        FitnessValues fv = fitness(t);

                        DataLogger.Log(LogTarget.BasicGeneticOutput, Variation,
                            $"{iter},{t.ID},{fv.time},{fv.fitness},{fv.linearity},{fv.pathLength}," +
                            $"{fv.numberOfPaths},{fv.nullSpace},{fv.walkableSurface}");
                        
                        sum += fv.fitness;
                    }
                    
                    _currentPopulation.Sort(Utility.CompareChromosome);
                    
                    // for last iteration, population does not need to be generated
                    if(iter == Iteration - 1) break;
                    
                    while (_runPopulation.Count < _currentPopulation.Count)
                    {
                        // Survivor selection
                        if (elites > 0) {
                            _runPopulation.Add(GeneticGeneticOperator.FitnessProportionateSelection
                                (_currentPopulation, sum));
                            elites--;
                        } else 
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

                            one = GeneticGeneticOperator.UniformMutation(one, MutationProbability, GenerateGene);
                            two = GeneticGeneticOperator.UniformMutation(two, MutationProbability, GenerateGene);
                            
                            double fit1 = fitness(one).fitness;
                            double fit2 = fitness(two).fitness;
                            
                            // pick which offspring is sent into the next population
                            if (fit1 > fit2)
                            {
                                _runPopulation.Add(one);
                            } else if (fit2 > fit1)
                            {
                                _runPopulation.Add(two);
                            }
                            else
                            {
                                _runPopulation.Add(RandomG.Next(0,1) == 1 ? one : two);
                            }
                        }
                    }
                    (_currentPopulation, _runPopulation) = (_runPopulation, _currentPopulation);
            }
            // find best-fitting chromosomes
            _currentPopulation.Sort(Utility.CompareChromosome);
            return _currentPopulation.Take(5).ToList();
        }
        
        public override void SaveInfo()
        {
            throw new NotImplementedException();
        }


        private void RemoveLessFitFromPopulation(List<Chromosome> population)
        {
            population.Sort(Utility.CompareChromosome);

            population.Remove(population[population.Count - 1]);
        }
    }
}