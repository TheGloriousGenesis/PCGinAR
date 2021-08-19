using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Behaviour.Entities;
using Generators;
using GeneticAlgorithms.Algorithms;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Parameter;
using PathFinding;
using PathFinding.LinkGenerator;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Utilities;
using Debug = UnityEngine.Debug;
using Random = System.Random;
#if UNITY_EDITOR

#endif

namespace GeneticAlgorithms.Implementation
{
    public class GaImplementation : MonoBehaviour
    {
        [Header("Platform variables")] 
        [Range(3, 15)][SerializeField] private int MaxPlatformWidth = Constants.MAX_PLATFORM_DIMENSION_X;
        [Range(3, 15)][SerializeField] private int MaxPlatformHeight = Constants.MAX_PLATFORM_DIMENSION_Y;
        [Range(3, 15)][SerializeField] private int MaxPlatformDepth = Constants.MAX_PLATFORM_DIMENSION_Z;

        [Header("Core Genetic Algorithm Variables")] [SerializeField]
        public AlgorithmType AlgorithmType = AlgorithmType.Basic;

        [SerializeField] private int Seed = Constants.SEED;
        [Range(5, 300)][SerializeField] private int PopulationSize = Constants.POPULATION_SIZE;
        [Range(5, 10)][SerializeField]
        private int ChromosomeLength = Constants.CHROMOSOME_LENGTH;
        [SerializeField] private int Elitism = Constants.ELITISM;
        [SerializeField] private int NumberOfGenerations = Constants.NUMBER_OF_GENERATIONS;

        [Header("Crossover Variables")] 
        [SerializeField]
        private float CrossoverProbability = Constants.CROSSOVER_PROBABILITY;

        [Header("Mutation Variables")] 
        [SerializeField]
        private float MutationProbability = Constants.MUTATION_PROBABILITY;

        [Range(1, 4)] 
        public int MaxNumberOfMutations = Constants.MAX_NUMBER_OF_MUTATIONS;

        [Header("Selection Variables")] 
        [SerializeField]
        private int K = (int) Math.Ceiling(Constants.POPULATION_SIZE * 0.25);

        [Header("Post Processing Variables")] [SerializeField]
        private int NumberOfCoins = 5;

        private GenerateGameService _gameService;
        private PathFinding3D _pathFinding;
        private Random _random;
        private BasicGeneticAlgorithm _ga;
        private FI2POP _fi2Pop;

        private string variation;

        public static List<GameData> lastPlays = new List<GameData>();

        #region Event subscriptions

        public void OnEnable()
        {
            EventManager.OnSendGameStats += SetUpGARun;
        }

        public void OnDisable()
        {
            EventManager.OnSendGameStats -= SetUpGARun;
        }

        #endregion

        private void Awake()
        {
            _gameService = GetComponent<GenerateGameService>();
            _pathFinding = GetComponent<PathFinding3D>();
            _random = new Random(Seed);
#if UNITY_EDITOR
            if (_ga == null) 
                _ga = new BasicGeneticAlgorithm(PopulationSize, ChromosomeLength, CrossoverProbability,
                    _random, Elitism, MutationProbability, MaxNumberOfMutations, NumberOfGenerations, K, MaxPlatformWidth,
                    MaxPlatformHeight, MaxPlatformDepth, FitnessFunction);
            if (_fi2Pop == null)
                _fi2Pop = new FI2POP(PopulationSize, ChromosomeLength, CrossoverProbability,
                    _random, Elitism, MutationProbability, MaxNumberOfMutations, NumberOfGenerations, K, MaxPlatformWidth,
                    MaxPlatformHeight, MaxPlatformDepth, GenerateLevel, FitnessFunction, ClearLevel);
#else 
            _ga = new BasicGeneticAlgorithm(PopulationSize, ChromosomeLength, CrossoverProbability,
                _random, Elitism, MutationProbability, MaxNumberOfMutations, NumberOfGenerations, K, MaxPlatformWidth,
                MaxPlatformHeight, MaxPlatformDepth, FitnessFunction);
            _fi2Pop = new FI2POP(PopulationSize, ChromosomeLength, CrossoverProbability,
                _random, Elitism, MutationProbability, MaxNumberOfMutations, NumberOfGenerations, K, MaxPlatformWidth,
                MaxPlatformHeight, MaxPlatformDepth, GenerateLevel, FitnessFunction, ClearLevel);
#endif
            variation = $"PS{PopulationSize}_CL{ChromosomeLength}_MP{MaxPlatformWidth}." +
                        $"{MaxPlatformHeight}.{MaxPlatformDepth}_" +
                        $"G{NumberOfGenerations}";
            
            _ga.Variation = $"PS{PopulationSize}_CL{ChromosomeLength}_MP{MaxPlatformWidth}." +
                            $"{MaxPlatformHeight}.{MaxPlatformDepth}_" +
                            $"G{NumberOfGenerations}";
            _fi2Pop.Variation = $"PS{PopulationSize}_CL{ChromosomeLength}_MP{MaxPlatformWidth}." +
                                $"{MaxPlatformHeight}.{MaxPlatformDepth}_" +
                                $"G{NumberOfGenerations}";
        }

        #region Run methods

        public Chromosome RunGA()
        {
            // ChromosomeLength = CaluculateChromosome(MaxPlatformWidth, MaxPlatformDepth, MaxPlatformHeight);

#if UNITY_EDITOR
            Awake();
#endif
            _ga.Variation = $"PS{PopulationSize}_CL{ChromosomeLength}_MP{MaxPlatformWidth}." +
                            $"{MaxPlatformHeight}.{MaxPlatformDepth}_" +
                            $"G{NumberOfGenerations}";
            List<Chromosome> finalResult = null;
            switch (AlgorithmType)
            {
                case (AlgorithmType.Basic):
                    finalResult = _ga.Run();
                    break;
                case (AlgorithmType.FI2POP):
                    finalResult = _fi2Pop.Run();
                    break;
                default:
                    ARDebugManager.Instance.LogWarning("No GA Algorithm has been chosen");
                    break;
            }

            if (finalResult != null)
            {
#if UNITY_EDITOR
                Debug.Log($"current fitness: {finalResult[0].Fitness}");
#endif
                GenerateLevel(finalResult[0]);
                return finalResult[0];
            }

            return null;
        }

        // private int CaluculateChromosome(int MAX_PLATFORM_DIMENSION_X, int MAX_PLATFORM_DIMENSION_Z, int MAX_PLATFORM_DIMENSION_Y)
        // {
        //     int TOTAL_VOLUME = (MAX_PLATFORM_DIMENSION_X + 2) *
        //                        (MAX_PLATFORM_DIMENSION_Z + 2) *
        //                        (MAX_PLATFORM_DIMENSION_Y + 2);
        //     
        //     int TOTAL_MOCK_VOLUME = TOTAL_VOLUME - 
        //                             (2 * (MAX_PLATFORM_DIMENSION_X * MAX_PLATFORM_DIMENSION_Y +
        //                                   MAX_PLATFORM_DIMENSION_X * MAX_PLATFORM_DIMENSION_Z +
        //                                   MAX_PLATFORM_DIMENSION_Y * MAX_PLATFORM_DIMENSION_Z + 
        //                                   2 * MAX_PLATFORM_DIMENSION_X +
        //                                   2 * MAX_PLATFORM_DIMENSION_Y +
        //                                   2 * MAX_PLATFORM_DIMENSION_Z +
        //                                   4));
        //     return (int) Math.Ceiling(TOTAL_MOCK_VOLUME *
        //                        (1.0/4.0));
        // }

        public void RunTest()
        {
            int tester = NumberOfGenerations;
#if UNITY_EDITOR
            Awake();
#endif
            List<Chromosome> finalResult = null;
            switch (AlgorithmType)
            {
                case (AlgorithmType.Basic):
                    while (tester > 0)
                    {
                        finalResult = _ga.Run();
                        tester--;
                    }
                    break;
                case (AlgorithmType.FI2POP):
                    finalResult = _fi2Pop.Run();
                    break;
                default:
                    ARDebugManager.Instance.LogWarning("No GA Algorithm has been chosen");
                    break;
            }
            
            if (finalResult != null)
            {
#if UNITY_EDITOR
                Debug.Log($"current fitness: {finalResult[0].Fitness}");
#endif
                GenerateLevel(finalResult[0]);
            }
            OnApplicationQuit();
        }

        private void ClearLevel()
        {
#if UNITY_EDITOR
                _gameService = GetComponent<GenerateGameService>();
                _pathFinding = GetComponent<PathFinding3D>();
#endif
            _gameService.ResetGame(Utility.SafeDestroyInEditMode);
            _ga = null;
            _pathFinding.ResetPathFinding();
            Chromosome.ResetCounter();
            _random = null;
        }

        private void GenerateLevel(Chromosome chromosome)
        {
            _gameService.CreateGame(new Vector3(), Quaternion.identity, chromosome, BlockType.AGENT);
            _gameService.PostProcessingAdjustments(NumberOfCoins);
        }
        #endregion

        #region Fitness Analysis

        private FitnessValues FitnessFunction(Chromosome chromosome)
        {
            Stopwatch timer = new Stopwatch();
            FitnessValues fv = new FitnessValues();
            Stopwatch stop = new Stopwatch();

            float score = 0;
            timer.Reset();
            timer.Start();
            GenerateLevel(chromosome);
            timer.Stop();
            fv.time = timer.Elapsed.TotalMilliseconds;
            
            // stop.Start();
            score += AnalyseAStarPath(fv);
            // stop.Stop();
            // Debug.Log($"A start time: {stop.Elapsed.TotalMilliseconds}");

            // stop.Reset();
            // stop.Start();
            score += AnalyseMultiPaths(fv);
            // stop.Stop();
            // Debug.Log($"Multipath time: {stop.Elapsed.TotalMilliseconds}");

            // stop.Reset();
            // stop.Start();
            score += PercentageOfWalkableSurfaces(fv);
            // stop.Stop();
            // Debug.Log($"Fitness Evaluation time: {stop.Elapsed.TotalMilliseconds}");

            // stop.Reset();
            // stop.Start();
            score += PercentageOfNullSpace(fv);
            // stop.Stop();
            // Debug.Log($"Null space time: {stop.Elapsed.TotalMilliseconds}");

            // score += CalculateVolumeUsed();
            // score += PercentageOfChromosomeUsed();
            // stop.Reset();
            // stop.Start();
            CalculateLinearity(fv);
            // stop.Stop();
            // Debug.Log($"Linearity time: {stop.Elapsed.TotalMilliseconds}");


            //todo change as max score has now changed
            // (x - min(x)) / (max(x) -min(x) --> https://stats.stackexchange.com/questions/70801/how-to-normalize-data-to-0-1-range
            float max = 7f;
            float min = -25.5f;
            score = (score - min) / (max - min);

            chromosome.Fitness = score;

            _gameService.ResetGame(Utility.SafeDestroyInEditMode);

            fv.fitness = score;

            return fv;
        }

        private float AnalyseAStarPath(FitnessValues fitnessValues)
        {
            NavMeshPath path = NavMeshLinksAutoPlacer.ContainsPath();
            float score = 0;
            // Check completable level
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                score += 1;
            }
            else
            {
                score -= 5f;
            }
            float pathLength = path.GetTotalDistance();

            List<int> _longestDimension = new List<int>() {MaxPlatformDepth, MaxPlatformHeight, MaxPlatformWidth};
            int longestDimension = _longestDimension[0];
            if (pathLength >= longestDimension)
            {
                score += 1;
            }

            fitnessValues.pathStatus = path.status;
            fitnessValues.pathLength = pathLength;
            return score;
        }

        private float AnalyseMultiPaths(FitnessValues fitnessValues)
        {
            // Check number of paths
            float score = 0;
            int numberOfPaths = _pathFinding.FindPaths().Count;

            if (numberOfPaths == 0)
            {
                score -= 20;
            } else if (numberOfPaths == 1)
            {
                score +=  1;
            }
            else if (numberOfPaths <= 5)
            {
                score += 2;
            }
            else if (numberOfPaths <= 10)
            {
                score += 3;
            }
            else if (numberOfPaths <= 20)
            {
                score -= 1f;
            }

            fitnessValues.numberOfPaths = numberOfPaths;
            return score;
        }

        private float PercentageOfWalkableSurfaces(FitnessValues fitnessValues)
        {
            float score = 0.0f;
            int numberBricksPlaced = Utility.GetGameMap()[BlockType.FREE_TO_WALK].Count +
                                     Utility.GetGameMap()[BlockType.BASIC_BLOCK].Count;

            float percentageOfWalkableSurface =
                Utility.GetGameMap()[BlockType.FREE_TO_WALK].Count / (float) numberBricksPlaced;

            if (percentageOfWalkableSurface >= 0.5)
            {
                score += 1;
            }
            else
            {
                score -= 0.5f;
            }

            fitnessValues.walkableSurface = percentageOfWalkableSurface;
            return score;
        }

        private float PercentageOfNullSpace(FitnessValues fitnessValues)
        {
            float score = 0.0f;
            int numberBricksPlaced = Utility.GetGameMap()[BlockType.FREE_TO_WALK].Count +
                                     Utility.GetGameMap()[BlockType.BASIC_BLOCK].Count;
        
            float used = 1 - numberBricksPlaced * 1f / Constants.TOTAL_VOLUME;
        
            fitnessValues.nullSpace = used;
        
            score += numberBricksPlaced * 1f / Constants.TOTAL_VOLUME;
            return score;
        }

        private void CalculateLinearity(FitnessValues fitnessValues)
        {
            Dictionary<float, int> tmp = new Dictionary<float, int>(new FloatEqualityComparer());
            List<Vector3> allBricks = Utility.GetGameMap()[BlockType.BASIC_BLOCK];
            allBricks.AddRange(Utility.GetGameMap()[BlockType.FREE_TO_WALK]);

            foreach (var i in allBricks)
            {
                if (tmp.ContainsKey(i.y))
                {
                    tmp[i.y] = tmp[i.y] + 1;
                }
                else
                {
                    tmp.Add(i.y, 1);
                }
            }

            int diffValues = tmp.GroupBy(pair => pair.Value).Count();

            float linearity = (diffValues * 1.0f) / allBricks.Count;
            fitnessValues.linearity = linearity;
        }

        // private float[] CalculateNewWeights(List<Chromosome> chromosomes, bool isNegativeWeighting)
        // {
        //     float[] weights = new float[Constants.NUMBER_OF_CHUNKS];
        //     List<int> allChunksInLevel = chromosomes.SelectMany(x => x.Genes)
        //         .Select(x => x.allele.chunkID).ToList();
        //     int totalChunks = allChunksInLevel.Count;
        //
        //     int weighting = isNegativeWeighting ? -1 : 1;
        //     foreach (int id in allChunksInLevel)
        //     {
        //         weights[id] += weighting;
        //     }
        //
        //     for (int i = 0; i < weights.Length; i++)
        //     {
        //         weights[i] = weights[i] / Constants.NUMBER_OF_CHUNKS;
        //     }
        //
        //     return weights;
        // }

        private float CalculateVolumeUsed()
        {
            float score = 0;
            int numberBricksPlaced = Utility.GetGameMap()[BlockType.FREE_TO_WALK].Count +
                                     Utility.GetGameMap()[BlockType.BASIC_BLOCK].Count;

            float used = numberBricksPlaced / (float) Constants.TOTAL_MOCK_VOLUME;
            if (used >= 0.5)
            {
                score += 1;
            }
            else
            {
                score -= 1f;
            }

            return score;
        }

        private float PercentageOfChromosomeUsed()
        {
            int numberBricksPlaced = Utility.GetGameMap()[BlockType.FREE_TO_WALK].Count +
                                     Utility.GetGameMap()[BlockType.BASIC_BLOCK].Count;

            return numberBricksPlaced / (float) (Constants.CHROMOSOME_LENGTH * 3);
        }

        #endregion

        // #region InGame data for GA

        private void SetUpGARun(GameData gameData)
        {
            GameData.SaveGameData(gameData);
            lastPlays.Add(gameData);
        }

//         private void AddInGameData(GameData gameData)
//         {
//             // lastPlays.Add(gameData);
//             // if (lastPlays.Count == 2)
//             // {
//             //     List<Chromosome> playedSolutions =
//             //         _ga._currentPopulation.FindAll(i =>
//             //             lastPlays.Select(x => x.chromosomeID).Contains(i.ID));
//             //     GoalNotReached(playedSolutions);
//                 
//                 // List<GameData> numberOfInGameJumps = lastPlays.Where(x => x.numberOfJumps > MaxPlatformHeight).ToList();
//                 // if (numberOfInGameJumps.Count == 2)
//                 // {
//                 //     _ga.PlatformHeight = Math.Abs(_ga.PlatformHeight - 3) < 0.01 || Math.Abs(_ga.PlatformHeight - 8) < 0.01 ? _ga.PlatformHeight : _ga.PlatformHeight - 1;
//                 // }
// #if !UNITY_EDITOR
//                 float max = lastPlays.Select(x => x.numberOfPhysicalMovement).Max();
//                 float min = lastPlays.Select(x => x.numberOfPhysicalMovement).Min();
//                 float median = (max - min) / 2;
//                 if (median < 15)
//                 {
//                     _ga.weightedRandomBag.UpdateWeights(CalculateNewWeights(playedSolutions, true));
//                     _ga.PlatformWidth = Math.Abs(_ga.PlatformWidth - 3) < 0.01 || Math.Abs(_ga.PlatformWidth - 8) < 0.01 ? _ga.PlatformWidth : _ga.PlatformWidth + 1;
//                 }
// #endif
//                 lastPlays.Clear();
//             }
        // }

        // private void GoalNotReached(List<Chromosome> playedSolutions)
        // {
        //     List<GameData> goalNotReached = lastPlays.Where(x => x.goalReached == false).ToList();
        //     if (goalNotReached.Count == 2)
        //     {
        //         // Remove chromosomes from current population;
        //         List<int> chromosomeIDs = goalNotReached.Select(x => x.chromosomeID).ToList();
        //         var chromosomes = playedSolutions.FindAll(x => chromosomeIDs.Contains(x.ID));
        //         // Increase weighting of linear chunks to make level easier
        //         double[] updatedWeights =
        //             _ga.weightedRandomBag.UpdateWeights(CalculateNewWeights(chromosomes, true));
        //         _ga.AddDataToResults_Weights(string.Join(",", updatedWeights.Select(w => w.ToString())));
        //         _ga._currentPopulation.RemoveAll(x => chromosomeIDs.Contains(x.ID));
        //         
        //         _ga.PlatformHeight = Math.Abs(_ga.PlatformHeight - 3) < 0.01 || Math.Abs(_ga.PlatformHeight - 8) < 0.01 ? _ga.PlatformHeight : _ga.PlatformHeight - 1;
        //     }
        //     else if (goalNotReached.Count == 0)
        //     {
        //         double[] updatedWeights =
        //             _ga.weightedRandomBag.UpdateWeights(CalculateNewWeights(playedSolutions, false));
        //         _ga.AddDataToResults_Weights(string.Join(",", updatedWeights.Select(w => w.ToString())));
        //     }
        // }

        // #endregion

        private void OnApplicationQuit()
        {
            _ga.OutputTestResults(LogTarget.BasicGeneticOutput);
            _ga.OutputTestResults_Time(LogTarget.Time);
            _ga.OutputTestResults_Weights(LogTarget.WeightedChunks);
            _ga.OutputTestResults_Time(LogTarget.Time);
        }

#if UNITY_EDITOR

        [CustomEditor(typeof(GaImplementation))]
        [CanEditMultipleObjects]
        public class GaImplementation_Editor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                if (GUILayout.Button("Start Generation"))
                {
                    foreach (var targ in targets)
                    {
                        ((GaImplementation) targ).RunGA();
                    }
                }
                
                if (GUILayout.Button("Run Test Generation"))
                {
                    foreach (var targ in targets)
                    {
                        ((GaImplementation) targ).RunTest();
                    }
                }

                if (GUILayout.Button("Clear Generation"))
                {
                    foreach (var targ in targets)
                    {
                        ((GaImplementation) targ).ClearLevel();
                    }
                }
            }
        }

#endif
    }
}