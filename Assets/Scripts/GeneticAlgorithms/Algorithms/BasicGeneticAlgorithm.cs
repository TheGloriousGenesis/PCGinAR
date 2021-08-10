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
        public List<Chromosome> _currentPopulation;
        private List<Chromosome> _runPopulation;
        public List<Chromosome> playedLevels;


        public BasicGeneticAlgorithm(int populationSize, int chromosomeLength, float crossoverProbability, Random randomG, 
            int elitism, float mutationProbability, int iteration, int k) : 
            base(populationSize, chromosomeLength, crossoverProbability, randomG, elitism, 
                mutationProbability, iteration, k)
        {
            this._currentPopulation = GenerateGenotype(populationSize);
            this._runPopulation = new List<Chromosome>(populationSize);
            this.playedLevels = new List<Chromosome>();
        }

        public override List<Chromosome> Run(Func<Chromosome, FitnessValues> fitness)
        {
            // _runPopulation.Clear();
            // playedLevels.Clear();

            var variation = $"Ps{this.PopulationSize}_Cl{this.ChromosomeLength}_Pd{Constants.MAX_PLATFORM_DIMENSION_X}." +
                            $"{Constants.MAX_PLATFORM_DIMENSION_Y}.{Constants.MAX_PLATFORM_DIMENSION_Z}_" +
                            $"G{Iteration}";
            
            
            while (Iteration > 0)
            {
                _runPopulation = new List<Chromosome>();
                
                // Sort fitness in these lists 
                playedLevels.Sort(Utility.CompareChromosome);
                _currentPopulation.Sort(Utility.CompareChromosome);
                
                // calculate fitness for test population.
                foreach (var t in _currentPopulation)
                {
                    FitnessValues fv = fitness(t);

                    ARLogger.Log(LogTarget.BasicGeneticOutput, variation,
                        $"{t.ID},{fv.time},{fv.fitness},{fv.linearity},{Iteration}");
                }
                
                // for last iteration, population does not need to be generated
                if (Iteration <= 0) break;

                for (var i = 0; i < _currentPopulation.Count; i++)
                {
                    // Add all levels that have been played to population
                    if (i < Elitism && i < playedLevels.Count)
                    {
                        _runPopulation.Add(playedLevels[i]);
                    } else if (i < Elitism && i <_currentPopulation.Count) // then add top performing levels in current population
                    {
                        _runPopulation.Add(_currentPopulation[i - playedLevels.Count]);
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
                List<Chromosome> tmpList = _currentPopulation;
                _currentPopulation = _runPopulation;
                _runPopulation = tmpList;
                Iteration = Iteration - 1;
            }

            Iteration = 5;
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