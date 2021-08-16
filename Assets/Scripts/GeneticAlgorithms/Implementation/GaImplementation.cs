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
        [Header("Platform variables")] [SerializeField]
        private int MaxPlatformWidth = Constants.MAX_PLATFORM_DIMENSION_X;

        [SerializeField] private int MaxPlatformHeight = Constants.MAX_PLATFORM_DIMENSION_Y;
        [SerializeField] private int MaxPlatformDepth = Constants.MAX_PLATFORM_DIMENSION_Z;

        [Header("Core Genetic Algorithm Variables")] [SerializeField]
        public AlgorithmType AlgorithmType;

        [SerializeField] private int Seed = Constants.SEED;
        [SerializeField] private int PopulationSize = Constants.POPULATION_SIZE;
        [Range(5, 600)]
        [SerializeField] private int ChromosomeLength = Constants.CHROMOSOME_LENGTH;
        [SerializeField] private int Elitism = Constants.ELITISM;
        [SerializeField] private int NumberOfGenerations = Constants.NUMBER_OF_GENERATIONS;

        [Header("Crossover Variables")] [SerializeField]
        private float CrossoverProbability = Constants.CROSSOVER_PROBABILITY;

        [Header("Mutation Variables")] [SerializeField]
        private float MutationProbability = Constants.MUTATION_PROBABILITY;
        [Range(0, 3)]
        public int MaxNumberOfMutations = Constants.MAX_NUMBER_OF_MUTATIONS;

        [Header("Selection Variables")] [SerializeField]
        private int K = (int) Math.Ceiling(Constants.POPULATION_SIZE * 0.25);

        [Header("Post Processing Variables")] [SerializeField]
        private int NumberOfCoins = 5;

        private GenerateGameService _gameService;
        private PathFinding3D _pathFinding;
        private Random _random;
        private BasicGeneticAlgorithm _ga;
        private FI2POP _fi2Pop;

        private string variation;

        private List<GameData> lastThreePlays = new List<GameData>();

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

            _ga = new BasicGeneticAlgorithm(PopulationSize, ChromosomeLength, CrossoverProbability,
                _random, Elitism, MutationProbability, MaxNumberOfMutations, NumberOfGenerations, K, MaxPlatformWidth,
                MaxPlatformHeight, MaxPlatformDepth, FitnessFunction);
            _fi2Pop = new FI2POP(PopulationSize, ChromosomeLength, CrossoverProbability,
                _random, Elitism, MutationProbability, MaxNumberOfMutations, NumberOfGenerations, K, MaxPlatformWidth,
                MaxPlatformHeight, MaxPlatformDepth, GenerateLevel, FitnessFunction, ClearLevel);
            variation = $"PS{PopulationSize}_CL{ChromosomeLength}_MP{MaxPlatformWidth}." +
                        $"{MaxPlatformHeight}.{MaxPlatformDepth}_" +
                        $"G{NumberOfGenerations}";
        }

        public Chromosome RunGA()
        {
            _ga.Variation = $"PS{PopulationSize}_CL{ChromosomeLength}_MP{MaxPlatformWidth}." +
                            $"{MaxPlatformHeight}.{MaxPlatformDepth}_" +
                            $"G{NumberOfGenerations}";
            List<Chromosome> finalResult = _ga.Run();
            return finalResult[1];
        }

        public void RunGAInEdit()
        {
            List<Chromosome> finalResult;
            switch (AlgorithmType)
            {
                case (AlgorithmType.Basic):
                    Awake();
                    _ga.Variation = $"PS{PopulationSize}_CL{ChromosomeLength}_MP{MaxPlatformWidth}." +
                                    $"{MaxPlatformHeight}.{MaxPlatformDepth}_" +
                                    $"G{NumberOfGenerations}";
                    finalResult = _ga.Run();
                    GenerateLevel(finalResult[0]);
                    break;
                case (AlgorithmType.FI2POP):
                    Awake();
                    _fi2Pop.Variation = $"PS{PopulationSize}_CL{ChromosomeLength}_MP{MaxPlatformWidth}." +
                                        $"{MaxPlatformHeight}.{MaxPlatformDepth}_" +
                                        $"G{NumberOfGenerations}";
                    finalResult = _fi2Pop.Run();
                    GenerateLevel(finalResult[0]);
                    break;
                default:
                    Debug.Log("No Algorithm has been chosen");
                    break;
            }
        }

        private void RemoveInEdit()
        {
            if (!_gameService)
                _gameService = GetComponent<GenerateGameService>();
            if (!_pathFinding)
                _pathFinding = GetComponent<PathFinding3D>();

            ClearLevel();
        }

        private void ClearLevel()
        {
            _gameService.ResetGame(Utility.SafeDestroyInEditMode);
            _ga = null;
            _pathFinding.ResetPathFinding();
            Chromosome.ResetCounter();
            _random = null;
        }

        private void GenerateLevel(Chromosome chromosome)
        {
            _gameService.CreateGameGA(BlockType.AGENT, chromosome);
            _gameService.PostProcessingAdjustments(NumberOfCoins);
        }

        #region Fitness Analysis

        private FitnessValues FitnessFunction(Chromosome chromosome)
        {
            Stopwatch timer = new Stopwatch();
            FitnessValues fv = new FitnessValues();

            float score = 0;

            timer.Start();
            GenerateLevel(chromosome);
            timer.Stop();

            fv.time = timer.Elapsed.TotalMilliseconds;
            score += AnalyseAStarPath(fv);
            score += AnalyseMultiPaths(fv);
            score += PercentageOfWalkableSurfaces(fv);
            score += PercentageOfNullSpace(fv);
            // score += CalculateVolumeUsed();
            // score += PercentageOfChromosomeUsed();
            CalculateLinearity(fv);

            //todo change as max score has now changed
            // (x - min(x)) / (max(x) -min(x) --> https://stats.stackexchange.com/questions/70801/how-to-normalize-data-to-0-1-range
            float max = 5f;
            float min = -20.5f;
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
                score -= 10f;
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
                score -= 10;
            }

            if (1 <= numberOfPaths && numberOfPaths <= 5)
            {
                score -= 2;
            }
            else if (6 <= numberOfPaths && numberOfPaths <= 10)
            {
                score += 1;
            }
            else if (11 <= numberOfPaths && numberOfPaths <= 20)
            {
                score += 0.5f;
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

        private float[] CalculateNewWeights(List<Chromosome> chromosomes, bool isNegativeWeighting)
        {
            float[] weights = new float[Constants.NUMBER_OF_CHUNKS];
            List<int> allChunksInLevel = chromosomes.SelectMany(x => x.Genes)
                .Select(x => x.allele.chunkID).ToList();
            int totalChunks = allChunksInLevel.Count;

            int weighting = isNegativeWeighting ? -1 : 1;
            foreach (int id in allChunksInLevel)
            {
                weights[id] += weighting;
            }

            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = weights[i] / Constants.NUMBER_OF_CHUNKS;
            }

            return weights;
        }

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

        #region InGame data for GA

        private void SetUpGARun(GameData gameData)
        {
            GameData.SaveGameData(gameData);
            switch (AlgorithmType)
            {
                case AlgorithmType.Basic:
                    AddInGameData(gameData);
                    break;
                case AlgorithmType.FI2POP:
                    break;
            }
        }

        private void AddInGameData(GameData gameData)
        {
            lastThreePlays.Add(gameData);
            if (lastThreePlays.Count == 3)
            {
                List<Chromosome> playedSolutions =
                    _ga._currentPopulation.FindAll(i => 
                        lastThreePlays.Select(x => x.chromosomeID).Contains(i.ID));
                List<GameData> goalNotReached = lastThreePlays.Where(x => x.goalReached == false).ToList();
                if (goalNotReached.Count >= 2)
                {
                    // Remove chromosomes from current population;
                    List<int> chromosomeIDs = goalNotReached.Select(x => x.chromosomeID).ToList();
                    var chromosomes = playedSolutions.FindAll(x => chromosomeIDs.Contains(x.ID));
                    // Increase weighting of linear chunks to make level easier
                    double[] updatedWeights = _ga.weightedRandomBag.UpdateWeights(CalculateNewWeights(chromosomes, true));
                    _ga.AddDataToResults_Weights(string.Join(",", updatedWeights.Select(w => w.ToString())));
                    _ga._currentPopulation.RemoveAll(x => chromosomeIDs.Contains(x.ID));
                }
                else
                {
                    double[] updatedWeights = _ga.weightedRandomBag.UpdateWeights(CalculateNewWeights(playedSolutions, false));
                    _ga.AddDataToResults_Weights(string.Join(",", updatedWeights.Select(w => w.ToString())));
                }

#if !UNITY_EDITOR
                float max = lastThreePlays.Select(x => x.numberOfPhysicalMovement).Max();
                float min = lastThreePlays.Select(x => x.numberOfPhysicalMovement).Min();
                float median = (max - min) / 2;
                if (median < 15)
                {
                    _ga.weightedRandomBag.UpdateWeights(CalculateNewWeights(playedSolutions, true));
                }
#endif
                lastThreePlays.Clear();
            }
        }
        #endregion

        private void OnApplicationQuit()
        {
            _ga.OutputTestResults_Weights(LogTarget.WeightedChunks);
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
                        ((GaImplementation) targ).RunGAInEdit();
                    }
                }

                if (GUILayout.Button("Clear Generation"))
                {
                    foreach (var targ in targets)
                    {
                        ((GaImplementation) targ).RemoveInEdit();
                    }
                }
            }
        }

#endif
    }
}