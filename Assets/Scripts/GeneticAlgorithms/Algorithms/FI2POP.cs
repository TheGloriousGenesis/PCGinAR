using System;
using System.Collections.Generic;
using GeneticAlgorithms.Entities;
using UnityEngine;
using UnityEngine.AI;
using Utilities;
using Random = System.Random;

// Inspired by
//https://github.com/prezolov/Level-Generator/blob/66f2975bbded2368db9745a15c25e73eaf76559d/Assets/Scripts/FI2PopGA.cs

namespace GeneticAlgorithms.Algorithms
{
    public class FI2POP : BaseAlgorithm
    {
        public List<Chromosome> infeasiblePopulation = new List<Chromosome>();
        public List<Chromosome> feasiblePopulation = new List<Chromosome>();

        // Used to keep track of all the feasible Individuals
        public List<Chromosome> feasibleIndividuals = new List<Chromosome>();

        private bool initialisedInfeasiblePop;
        private bool initialisedFeasiblePop;
        private int feasibleIndividualGeneration;

        public Chromosome infeasibleFittest = new Chromosome();
        public Chromosome feasibleFittest = new Chromosome();

        public float fittestInfeasible = 0;
        public float fittestFeasible = 0;
        public float fitnessInfeasible = 0;
        public float fitnessFeasible = 0;
        public int currentFeasibleIndividual = 0;
        public int currentInfeasibleIndividual = 0;

        protected int firstFeasibleGeneration;
        public int feasibleIndividualCount;

        public Action<Chromosome> generateLevel;
        public Func<Chromosome, FitnessValues> fitness;
        public Action clearLevel;

        public FI2POP(int populationSize, int chromosomeLength, float crossoverProbability, Random randomG,
            int elitism, float mutationProbability, int maxNumberOfMutations, int iteration, int k, int x, int y, int z,
            Action<Chromosome> generateLevel, Func<Chromosome, FitnessValues> fitness, Action clearLevel) :
            base(populationSize, chromosomeLength, crossoverProbability, randomG, elitism,
                mutationProbability,maxNumberOfMutations, iteration, k, x, y, z, fitness)
        {
            Initialise();
            this.fitness = fitness;
            this.generateLevel = generateLevel;
            this.clearLevel = clearLevel;
        }

        public void Initialise()
        {
            initialisedInfeasiblePop = false;
            initialisedFeasiblePop = false;
            infeasiblePopulation = new List<Chromosome>();
            feasiblePopulation = new List<Chromosome>();
            infeasiblePopulation = GenerateGenotype(PopulationSize);

            fittestFeasible = 0;
            fittestInfeasible = 0;
        }

        public override List<Chromosome> Run()
        {
            totalTime = 0;
            timer.Start();
            for (int iter = 0; iter < Iteration; ++iter)
            {
                totalTime += Time.deltaTime;
                // Will spawn infeasible levels and evaluate them
                if (!initialisedInfeasiblePop)
                    DisplayInfeasiblePopulation(infeasiblePopulation, iter);

                // Will spawn feasible levels and evaluate them
                if (feasiblePopulation.Count >= 1 && initialisedInfeasiblePop)
                {
                    DisplayFeasiblePopulation(feasiblePopulation, iter);
                }

                if (initialisedInfeasiblePop && (initialisedFeasiblePop || feasiblePopulation.Count == 0))
                {
                    // Remove weakest individuals from infeasible population to maintain population size
                    while (infeasiblePopulation.Count > PopulationSize)
                    {
                        infeasiblePopulation.RemoveAt(infeasiblePopulation.GetWeakestIndex());
                    }

                    // Delete individuals in feasible population which became infeasible from evolution
                    feasiblePopulation.RemoveAll(x => x.delete == true);

                    // Evolve infeasible population
                    infeasiblePopulation = EvolvePopulation(infeasiblePopulation);
                    currentInfeasibleIndividual = 0;
                    initialisedInfeasiblePop = false;
                    // Evolve feasible population, if it exists
                    if (initialisedFeasiblePop)
                    {
                        feasiblePopulation = EvolvePopulation(feasiblePopulation);
                        currentFeasibleIndividual = 0;
                        initialisedFeasiblePop = false;
                    }
                }
                if (csv_ga.Length == 0)
                {
                    AddDataToResults(string.Format("{0},{1},{2},{3},{4}", "Generation","Number of Feasible individuals",
                        "Fittest Individual Fitness",
                        "Generation of Fittest Individual", "Generation of First Feasible Individual"));
                }
                
                // Append new line to csv
                AddDataToResults(string.Format("{0},{1},{2},{3},{4}", iter, feasibleIndividualCount,
                    feasibleFittest.Fitness, feasibleIndividualGeneration, firstFeasibleGeneration));

                // else if (!finished)
                // {
                //     if (testing && currentTestRun < testRuns)
                //     {
                //         Debug.Log("Current run produced " + feasibleIndividualCount +
                //                   " feasible individuals, with a best fitness of " + feasibleFittest.Fitness +
                //                   ", generated at generation #" + feasibleIndividualGeneration);
                //         currentTestRun++;
                //
                //         if (csv.Length == 0)
                //         {
                //             AddDataToResults(string.Format("{0},{1},{2},{3}", "Number of Feasible individuals",
                //                 "Fittest Individual Fitness",
                //                 "Generation of Fittest Individual", "Generation of First Feasible Individual"));
                //         }
                //
                //         // Append new line to csv
                //         AddDataToResults(string.Format("{0},{1},{2},{3}", feasibleIndividualCount,
                //             feasibleFittest.Fitness, feasibleIndividualGeneration, firstFeasibleGeneration));
                //
                //         clearLevel.Invoke();
                //
                //         if (currentTestRun <= testRuns - 1)
                //         {
                //             infeasiblePopulation.Clear();
                //             feasiblePopulation.Clear();
                //             infeasibleFittest = new Chromosome();
                //             feasibleFittest = new Chromosome();
                //             feasibleIndividualCount = 0;
                //             Initialise();
                //         }
                //
                //         totalTime = 0;
                //         currentInfeasibleIndividual = 0;
                //         currentFeasibleIndividual = 0;
                //
                //
                //         initialisedInfeasiblePop = false;
                //         initialisedFeasiblePop = false;
                //         fittestFeasible = 0;
                //         fittestInfeasible = 0;
                //     }
                //     else
                //     {
                //         clearLevel.Invoke();
                //
                //         if (feasibleIndividualCount > 0)
                //         {
                //             Debug.Log("Clearing and spawning fittest, it has a fitness of: " + feasibleFittest.Fitness);
                //             generateLevel(feasibleFittest);
                //         }
                //         else
                //         {
                //             Chromosome fittest = infeasiblePopulation.GetFittest();
                //             Debug.Log(
                //                 "No feasible individual was found, clearing and spawning fittest from infeasible population, it has a fitness of: " +
                //                 fittest.Fitness);
                //             generateLevel(fittest);
                //         }
                //
                //         if (testing)
                //         {
                //             OutputTestResults(LogTarget.FI2POP);
                //         }
                //
                //         finished = true;
                //     }
                // }
            }
            timer.Stop();
            totalTime = timer.ElapsedMilliseconds;
            Debug.Log($"Total time F12POP (ms): {totalTime}");
            OutputTestResults(LogTarget.FI2POP);
            return feasiblePopulation;
        }

        // Displays the infeasible population in Unity
        void DisplayInfeasiblePopulation(List<Chromosome> pop, int generation)
        {
            if (currentInfeasibleIndividual < pop.Count)
            {
                FitnessValues fv = fitness(pop[currentInfeasibleIndividual]);

                // constraint fitness
                pop[currentInfeasibleIndividual].Fitness = fv.walkableSurface;

                fitnessInfeasible = pop[currentInfeasibleIndividual].Fitness;

                if (fitnessInfeasible >= fittestInfeasible)
                {
                    fittestInfeasible = fitnessInfeasible;
                    infeasibleFittest = Utility.DeepClone(pop[currentInfeasibleIndividual]);
                }

                // Feasible
                if (fv.pathStatus == NavMeshPathStatus.PathComplete && fv.walkableSurface >= 0.6)
                {
                    // Keep track of generation of first feasible individual
                    if (feasibleIndividualCount == 0)
                        firstFeasibleGeneration = generation;

                    // Count unique feasible individuals, not all of them
                    if (!feasibleIndividuals.Contains(pop[currentInfeasibleIndividual]))
                    {
                        feasibleIndividualCount++;
                        feasibleIndividuals.Add(Utility.DeepClone(pop[currentInfeasibleIndividual]));
                    }

                    // Create a copy with the new feasible individual
                    Chromosome feasibleIndividual = Utility.DeepClone(pop[currentInfeasibleIndividual]);

                    if (feasiblePopulation.Count < PopulationSize)
                    {
                        // Simply add to feasible population
                        feasiblePopulation.Add(feasibleIndividual);
                    }
                    else
                    {
                        // Replace weakest feasible individual with new feasible individual
                        if (feasiblePopulation.GetWeakest().Fitness > fv.fitness)
                            feasiblePopulation[feasiblePopulation.GetWeakestIndex()] = feasibleIndividual;
                    }
                }

                currentInfeasibleIndividual++;
                if (currentInfeasibleIndividual == infeasiblePopulation.Count)
                {
                    initialisedInfeasiblePop = true;
                }
            }
        }


        // Displays the feasible population in Unity
        void DisplayFeasiblePopulation(List<Chromosome> pop, int generation)
        {
            if (currentFeasibleIndividual < pop.Count)
            {
                FitnessValues fv = fitness(pop[currentFeasibleIndividual]);

                pop[currentFeasibleIndividual].Fitness = fv.fitness;

                // Infeasible
                if (fv.pathStatus != NavMeshPathStatus.PathComplete || fv.walkableSurface < 0.6)
                {
                    // Since individual has now become infeasible, set it's fitness to the constraint fitness
                    pop[currentFeasibleIndividual].Fitness = fv.walkableSurface;

                    // Move to infeasible population
                    infeasiblePopulation.Add(Utility.DeepClone(pop[currentFeasibleIndividual]));
                    // Set to delete
                    pop[currentFeasibleIndividual].delete = true;
                }
                else
                {
                    // Count unique feasible individuals, not all of them
                    if (!feasibleIndividuals.Contains(pop[currentFeasibleIndividual]))
                    {
                        feasibleIndividualCount++;
                        feasibleIndividuals.Add(Utility.DeepClone(pop[currentFeasibleIndividual]));
                    }

                    fitnessFeasible = pop[currentFeasibleIndividual].Fitness;
                    if (fitnessFeasible > fittestFeasible)
                    {
                        feasibleIndividualGeneration = generation;
                        fittestFeasible = fitnessFeasible;
                        feasibleFittest = Utility.DeepClone(pop[currentFeasibleIndividual]);
                    }
                }

                currentFeasibleIndividual++;
                if (currentFeasibleIndividual == feasiblePopulation.Count)
                {
                    initialisedFeasiblePop = true;
                }
            }
        }


        private List<Chromosome> EvolvePopulation(List<Chromosome> currentPopulation)
        {
            int elites = Elitism;
            List<Chromosome> runPopulation = new List<Chromosome>();
            while (runPopulation.Count < currentPopulation.Count)
            {
                // Survivor selection
                if (elites > 0)
                {
                    runPopulation.Add(GeneticGeneticOperator.FitnessProportionateSelection
                        (currentPopulation, 0));
                    elites--;
                }
                else
                {
                    var parents = GeneticGeneticOperator.TournamentSelection(currentPopulation);
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

                    runPopulation.Add(one);
                    runPopulation.Add(two);
                }
            }

            return runPopulation;
        }
    }
}