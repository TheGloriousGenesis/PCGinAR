using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Parameter;
using Utilities;

namespace GeneticAlgorithms.Algorithms
{
    [Serializable]
    public class BasicGeneticAlgorithm : BaseAlgorithm
    {
        private List<Chromosome> _currentPopulation;
        private List<Chromosome> _runPopulation;
        
        public BasicGeneticAlgorithm(int populationSize, int chromosoneLength, float crossoverProbability, Random randomG, 
            int elitism, float mutationProbability, int iteration, int k) : 
            base(populationSize, chromosoneLength, crossoverProbability, randomG, elitism, mutationProbability, iteration,
                k)
        {
            _currentPopulation = GenerateGenotype(populationSize);
            _runPopulation = new List<Chromosome>(populationSize);
        }

        public override List<Chromosome> Run(Func<Chromosome, double> fitness)
        {
            _runPopulation.Clear();

            var variation = $"Ps{this.PopulationSize}_Cl{this.ChromosoneLength}_Pd{Constants.MAX_PLATFORM_DIMENSION}_" +
                            $"G{Iteration}";

            for (var iter = 0; iter < Iteration; iter++)
            {
                _runPopulation = new List<Chromosome>();

                // calculate fitness for test population.
                foreach (var t in _currentPopulation)
                {
                    double time = fitness(t);
                    
                    Logger.Log(LogTarget.BasicGeneticOutput, variation,
                        $"{t.ID},{time},{t.Fitness},{iter}");
                }
                
                // for last iteration, population does not need to be generated
                if (iter == Iteration - 1) break;

                while (_runPopulation.Count < _currentPopulation.Count)
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

                    one = GeneticGeneticOperator.UniformMutation(one, MutationProbability,GenerateGene);
                    two = GeneticGeneticOperator.UniformMutation(two, MutationProbability,GenerateGene);

                    _runPopulation.Add(one);
                    _runPopulation.Add(two);
                }
                List<Chromosome> tmpList = _currentPopulation;
                _currentPopulation = _runPopulation;
                _runPopulation = tmpList;
            }
            // find best-fitting chromosomes
            _currentPopulation.Sort(Utility.CompareChromosome);
            return _currentPopulation.Take(5).ToList();
        }

        public override void SaveInfo()
        {
            throw new NotImplementedException();
        }
    }
}