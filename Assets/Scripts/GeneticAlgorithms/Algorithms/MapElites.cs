// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
// using GeneticAlgorithms.Entities;
//
// namespace GeneticAlgorithms.Algorithms
// {
//     [Serializable]
//     public class MapElites : BaseAlgorithm
//     {
//         private Chromosome[,,] _mapOfElites;
//         private double[,,] _mapOfElitesFitness;
//         // this N must match the feature space variations
//         private int _n;
//         private List<Chromosome> _currentPopulation;
//         private FeatureDimension[] _featureDimensions;
//
//         public MapElites(int populationSize, int chromosomeLength, float crossoverProbability, Random randomG, 
//             int elitism, float mutationProbability,int iteration, int k, int N, int x, int y, int z,
//             Func<Chromosome, FitnessValues> fitness) :
//             base(populationSize, chromosomeLength, crossoverProbability, randomG, elitism, 
//                 mutationProbability, iteration, k, x, y, z, fitness)
//         {
//             _n = N;
//             _mapOfElites = new Chromosome[N, N, N];
//             _mapOfElitesFitness = new double[N, N, N];
//             _currentPopulation = GenerateGenotype(populationSize);
//             _featureDimensions = new FeatureDimension[N];
//             InstantiateFeatures();
//         }
//
//         private void InstantiateFeatures()
//         {
//             _featureDimensions[0] = new FeatureDimension("LengthOfPath");
//             _featureDimensions[1] = new FeatureDimension("NumberOfCorners");
//             _featureDimensions[2] = new FeatureDimension("NumberOfJumps");
//         }
//
//         public override List<Chromosome> Run()
//         {
//             var gaTimer = new Stopwatch();
//             
//             gaTimer.Start();
//             const int g = 10;
//
//             for (var i = 0; i < Iteration; i++)
//             {
//                 Chromosome one;
//                 if (i < g)
//                 {
//                     var parents = GeneticGeneticOperator.TournamentSelection(_currentPopulation);
//                     var tmp = GeneticGeneticOperator.SinglePointCrossover(parents[0], parents[1]);
//                     one = tmp[0];
//                 }
//                 else
//                 {
//                     one = _currentPopulation[RandomG.Next()];
//                     one = GeneticGeneticOperator.UniformMutation(one, MutationProbability, GenerateGene, MaxNumberOfMutations);
//                 }
//                 PlaceInMapElites(one, FitnessFunction);
//             }
//             gaTimer.Stop();
//             return new List<Chromosome>(){GetMostPromisingSolution()};
//         }
//
//         private Chromosome GetMostPromisingSolution()
//         {
//             var bestPerformance = _mapOfElitesFitness.Cast<float>().Max();
//             
//             var bestPerformanceIndex = _mapOfElitesFitness.Cast<float>().ToList().IndexOf(bestPerformance);  
//             
//             var chromo = _mapOfElites.Cast<Chromosome>().ToArray()[bestPerformanceIndex];
//
//             return chromo;
//
//         }
//         
//         // fitness == to performance
//         public void PlaceInMapElites(Chromosome one, Func<Chromosome, FitnessValues> fitness)
//         {
//             var _fitness = fitness(one);
//             // Logger.Log(Logger.Log(LogTarget.MapElitesOutput, $"{currentPopulation[i].id_},{time}," +
//             //                                                  $"{currentPopulation[i].fitness},{iter}");
//             var getIndices = GetFeatureValue(one);
//             var currentFitnessInCell = _mapOfElitesFitness[getIndices[0], getIndices[1], getIndices[2]];
//             if (currentFitnessInCell != 0.0f && !(currentFitnessInCell < _fitness.fitness)) return;
//             _mapOfElitesFitness[getIndices[0], getIndices[1], getIndices[2]] = _fitness.fitness;
//             _mapOfElites[getIndices[0], getIndices[1], getIndices[2]] = one;
//         }
//
//         private int[] GetFeatureValue(Chromosome chromosome)
//         {
//             var featureValues = new int[_n];
//             for (var i =0; i < _n; i++)
//             {
//                 var tmp = _featureDimensions[i].Calculate(chromosome);
//                 featureValues[i] = _featureDimensions[i].Discretize(tmp);
//             }
//             return featureValues;
//         }
//     }
// }