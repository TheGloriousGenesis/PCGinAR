using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Implementation;
using GeneticAlgorithms.Parameter;
using UI;
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
        private int CurrentNumberOfRuns = 0;
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
        }

        public override List<Chromosome> Run()
        {
            Debug.Log(ChromosomeLength);
            CurrentNumberOfRuns++;

            // for last iteration, population does not need to be generated
            if (CurrentNumberOfRuns > Iteration)
            {
                ARDebugManager.Instance.LogInfo("Thats all folks!");
                OutputTestResults(LogTarget.BasicGeneticOutput);
                OutputTestResults_Weights(LogTarget.WeightedChunks);
                OutputTestResults_Time(LogTarget.Time);
                CurrentNumberOfRuns = 0;
                return null;
            }
            
            totalTime = 0;
            timer.Reset();
            timer.Start();
            
            // generate new individuals with new weights. Keep top performing of previous run
            List<Chromosome> tmp = _currentPopulation;
            List<Chromosome> updatedIndividuals = new List<Chromosome>();
            int count = K;
            while (count > 0)
            {
                tmp.Remove(_currentPopulation.GetWeakest());
                updatedIndividuals.Add(GenerateChromosome());
                count--;
            }
            tmp.AddRange(updatedIndividuals);
            _currentPopulation = tmp;
            
            float sum = 0.0f;

            for (int iter = 0; iter < 5; ++iter)
            {
                totalTime += Time.deltaTime;

                _runPopulation = new List<Chromosome>();

                int elites = Elitism;
                //
                // stop.Reset();
                // stop.Start();
                // calculate fitness for test population.
                sum = 0.0f;
                foreach (var t in _currentPopulation)
                {
                    FitnessValues fv = FitnessFunction(t);

                    AddDataToResults(
                        $"{CurrentNumberOfRuns},{t.ID},{fv.time},{fv.fitness},{fv.linearity},{fv.pathLength}," +
                        $"{fv.numberOfPaths},{fv.nullSpace},{fv.walkableSurface}");
                    sum += fv.fitness;
                }
                // stop.Stop();
                // Debug.Log($"Population fitness timing: {stop.Elapsed.TotalMilliseconds} ms");

                if (CurrentNumberOfRuns % 3 == 0)
                {
                    ModifyFitnessFunction();
                }

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

                        one = GeneticGeneticOperator.UniformMutation(one, MutationProbability, GenerateGene,
                            MaxNumberOfMutations);
                        two = GeneticGeneticOperator.UniformMutation(two, MutationProbability, GenerateGene,
                            MaxNumberOfMutations);
                        _runPopulation.Add(one);
                        _runPopulation.Add(two);
                    }
                }

                (_currentPopulation, _runPopulation) = (_runPopulation, _currentPopulation);
            }

            timer.Stop();
            totalTime = timer.ElapsedMilliseconds;
            AddDataToResults_Time($"{CurrentNumberOfRuns},{totalTime},{PlatformHeight},{PlatformWidth}");
            
            // find best-fitting chromosomes
            _currentPopulation.Sort(Utility.CompareChromosome);
            return _currentPopulation.ToList();
        }

        private void ModifyFitnessFunction()
        {
            if (GaImplementation.lastPlays.Count == 0)
            {
                Debug.Log("Nothing to modify");
                return;
            }
            int totalLastPlays = GaImplementation.lastPlays.Count;
            double timingAverage = GaImplementation.lastPlays.Select(x =>x.timeCompleted).Sum() * 1.0f/totalLastPlays;
            float physicalMovementAverage = GaImplementation.lastPlays.Select(x =>x.numberOfPhysicalMovement).Sum() * 1.0f/totalLastPlays;
            float successfulGoalReaches = GaImplementation.lastPlays.Select(x =>x.goalReached == true).Count() * 1.0f/totalLastPlays;
            float numberOfInGameJumps = GaImplementation.lastPlays.Select(x => x.numberOfJumps).Sum() * 1.0f/totalLastPlays;
            if (successfulGoalReaches > 0.5)
            {
                if (physicalMovementAverage > 8.6 || numberOfInGameJumps > 5)
                {
                    if (timingAverage < 10000 )
                    {
                        Incentivise("Nonlinear");
                    }
                }
                else
                {
                    Incentivise("Nonlinear");
                }
                
            }
            else
            {
                Incentivise("Linear");
            }
        }

        private void Incentivise(string linear)
        {
            float[] weights = new float[Constants.NUMBER_OF_CHUNKS];
            int[] linearIndices = new int[] {10, 11, 13, 14, 5, 6 };
            int[] nonLinearIndices = new int[] {0, 1,2,3,4,7,8,9,12,15};
            switch(linear)
            {
                case "Linear":
                    foreach (int i in linearIndices)
                    {
                        weights[i] = 1;
                    }
                    ChromosomeLength = ChromosomeLength == 3 || ChromosomeLength == 8 ? ChromosomeLength : ChromosomeLength--;
                    break;
                case "Nonlinear":
                    foreach (int i in nonLinearIndices)
                    {
                        weights[i] = 1;
                    }
                    ChromosomeLength = ChromosomeLength == 3 || ChromosomeLength == 8 ? ChromosomeLength : ChromosomeLength++;
                    break;
            }
            weightedRandomBag.UpdateWeights(weights);
        }
        
    }
}