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
        [SerializeField]
        private int MaxPlatformWidth = Constants.MAX_PLATFORM_DIMENSION_X;
        [SerializeField]
        private int MaxPlatformHeight = Constants.MAX_PLATFORM_DIMENSION_Y;
        [SerializeField]
        private int MaxPlatformDepth = Constants.MAX_PLATFORM_DIMENSION_Z;

        [Header("Core Genetic Algorithm Variables")] 
        [SerializeField]
        private int Seed = Constants.SEED;
        [SerializeField]
        private int PopulationSize = Constants.POPULATION_SIZE;
        [SerializeField]
        private int ChromosomeLength = Constants.CHROMOSOME_LENGTH;
        [SerializeField]
        private int Elitism = Constants.ELITISM;
        [SerializeField]
        private int NumberOfGenerations = Constants.NUMBER_OF_GENERATIONS;
        
        [Header("Crossover Variables")]
        [SerializeField]
        private float CrossoverProbability = Constants.CROSSOVER_PROBABILITY;
        
        [Header("Mutation Variables")]
        [SerializeField]
        private float MutationProbability = Constants.MUTATION_PROBABILITY;
        
        [Header("Selection Variables")] 
        [SerializeField]
        private int K = Constants.K;

        [Header("Post Processing Variables")] 
        [SerializeField]
        private int NumberOfCoins = 5;

        private GenerateGameService _gameService;
        private PathFinding3D _pathFinding;
        private Random _random;
        private BasicGeneticAlgorithm _ga;

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
                _random, Elitism, MutationProbability, NumberOfGenerations, K, MaxPlatformWidth, 
                MaxPlatformHeight, MaxPlatformDepth);
        }

        public Chromosome Run()
        {
            _ga.Variation = $"PS{PopulationSize}_CL{ChromosomeLength}_MP{MaxPlatformWidth}." +
                            $"{MaxPlatformHeight}.{MaxPlatformDepth}_" +
                            $"G{NumberOfGenerations}";
            List<Chromosome> finalResult = _ga.Run(FitnessFunction);
            return finalResult[1];
        }
        
        public void RunInEdit()
        {
            Awake();
            _ga.Variation = $"PS{PopulationSize}_CL{ChromosomeLength}_MP{MaxPlatformWidth}." +
                            $"{MaxPlatformHeight}.{MaxPlatformDepth}_" +
                            $"G{NumberOfGenerations}";
            List<Chromosome> finalResult = _ga.Run(FitnessFunction);
            _gameService.CreateGameGA(BlockType.AGENT, finalResult[0]);
            _gameService.PostProcessingAdjustments(NumberOfCoins);
        }

        private void RemoveInEdit()
        {
            if (!_gameService)
                _gameService = GetComponent<GenerateGameService>();
            if (!_pathFinding)
                _pathFinding = GetComponent<PathFinding3D>();
            _gameService.ResetGame(Utility.SafeDestroyInEditMode);
            _ga = null;
            _pathFinding.ResetPathFinding();
            Chromosome.ResetCounter();
            _random = null;
        }

        private void Remove()
        {
            _gameService.ResetGame(Utility.SafeDestroyInEditMode);
            _ga = null;
            _pathFinding.ResetPathFinding();
            Chromosome.ResetCounter();
            _random = null;
        }

        #region Fitness Analysis
        private FitnessValues FitnessFunction(Chromosome chromosome)
        {
            Stopwatch timer = new Stopwatch();
            FitnessValues fv = new FitnessValues();
            
            float score = 0;

            timer.Start();
            _gameService.CreateGameGA(BlockType.AGENT, chromosome);
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
            float max = 4f;
            float min = -12.5f;
            score = (score - min)/(max - min);

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

            fitnessValues.pathLength = path.GetTotalDistance();
            return score;
        }
        private float AnalyseMultiPaths(FitnessValues fitnessValues)
        {
            // Check number of paths
            float score = 0;
            int numberOfPaths =  _pathFinding.FindPaths().Count;
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
            
            float percentageOfWalkableSurface = Utility.GetGameMap()[BlockType.FREE_TO_WALK].Count / (float) numberBricksPlaced;

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
        private float[] CalculateNewWeights(Chromosome chromosome)
        {
            float[] weights = new float[Constants.NUMBER_OF_CHUNKS];
            List<int> allChunksInLevel = chromosome.Genes.Select(x => x.allele.chunkID).ToList();
            int totalChunks = allChunksInLevel.Count;
 
            foreach (int id in allChunksInLevel)
            {
                weights[id] += 1f;
            }
            
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = (weights[i] / totalChunks) * (1f/Constants.NUMBER_OF_CHUNKS);
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
            var chromosomes = _ga._currentPopulation.FindAll(x => x.ID == gameData.chromosomeID);
            if (chromosomes.Count > 1)
                Debug.Log($"More than 1 Chromosome found with id {gameData.chromosomeID}");
            var chromosome = _ga._currentPopulation.Find(x => x.ID == gameData.chromosomeID);
            
            float[] currentWeights = CalculateNewWeights(chromosome);

            GameData.SaveGameData(gameData);

            if (!_ga.goodPlayedLevels.Contains(chromosome))
            {
                _ga.goodPlayedLevels.Add(chromosome);
            }
            _ga.weightedRandomBag.UpdateWeights(currentWeights);
            
            if (gameData.goalReached != true)
            {
                _ga._currentPopulation.RemoveAll(x => x.ID == gameData.chromosomeID);
            }
            // if (gameData.timeCompleted > 13906.8)
            // {
            //     Constants.CHROMOSONE_LENGTH = 2;
            // }
            if (gameData.numberOfJumps < 4)
            {
                
                Constants.CHROMOSOME_LENGTH = 5;
            }
            
            
        }

        #endregion

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
                        ((GaImplementation)targ).RunInEdit();
                    }
                }

                if (GUILayout.Button("Clear Generation"))
                {
                    foreach (var targ in targets)
                    {
                        ((GaImplementation)targ).RemoveInEdit();
                    }
                }
            }
        }

#endif
    }
}
