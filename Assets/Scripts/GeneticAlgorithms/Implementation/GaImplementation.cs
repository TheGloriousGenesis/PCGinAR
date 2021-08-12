using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Behaviour.Entities;
using Generators;
using GeneticAlgorithms.Algorithms;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Parameter;
using PathFinding;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Utilities;
using Debug = UnityEngine.Debug;
using Random = System.Random;

#if UNITY_EDITOR

#endif

namespace GeneticAlgorithms.Implementation
{
    public class GaImplementation : MonoBehaviour
    {
        [Header("Core Genetic Algorithm Variables")] 
        [SerializeField]
        private int Seed = Constants.SEED;
        [SerializeField]
        private int PopulationSize = Constants.POPULATION_SIZE;
        [SerializeField]
        private int ChromosomeLength = Constants.CHROMOSONE_LENGTH;
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
            if (_random == null) _random = new Random(Seed);
            
            if (_ga == null) _ga = new BasicGeneticAlgorithm(PopulationSize, ChromosomeLength, CrossoverProbability,
                _random, Elitism, MutationProbability, NumberOfGenerations, K);
        }

        public Chromosome Run()
        {
            List<Chromosome> finalResult = _ga.Run(FitnessFunction);
            return finalResult[1];
        }
        
        public void RunInEdit()
        {
            Awake();
            List<Chromosome> finalResult = _ga.Run(FitnessFunction);
            _gameService.CreateGameGA(BlockType.AGENT, finalResult[2]);
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
            float score = 0;

            timer.Start();
            _gameService.CreateGameGA(BlockType.AGENT, chromosome);
            timer.Stop();

            score += AnalyseAStarPath();
            
            score += AnalyseMultiPaths();
            score += CalculateVolumeUsed();
            score += PercentageOfChromosomeUsed();
            score += PercentageOfChromosomeThatIsNullSpace();
            score += PercentageOfWalkableSurfaces();
            float linear = CalculateLinearity();

            //todo change as max score has now changed
            // (x - min(x)) / (max(x) -min(x) --> https://stats.stackexchange.com/questions/70801/how-to-normalize-data-to-0-1-range
            float max = 7f;
            float min = -11f;
            score = (score - min)/(max - min);

            chromosome.Fitness = score;

            _gameService.ResetGame(Utility.SafeDestroyInEditMode);

            return new FitnessValues() {time = timer.Elapsed.TotalMilliseconds,
                fitness = score, linearity = linear};
        }

        private float AnalyseAStarPath()
        {
            NavMeshPath path = GenerateGameService.PathStatus();
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

            // todo: Have removed entropy calculate length in order to replace with ingame stats
            // // Check entropy
            // int numberOfTurns = path.corners.Length;
            // //todo: double check this 
            // // max number of turns is if a turn must be done on each block
            // int maxNumberOfTurns = Utility.GamePlacement.Keys.Count;
            // score += numberOfTurns/ (float) maxNumberOfTurns;

            return score;
        }
        private float AnalyseMultiPaths()
        {
            // Check number of paths
            float score = 0;
            int numberOfPaths =  _pathFinding.FindPaths().Count;
            if (1 <= numberOfPaths && numberOfPaths <= 5)
            {
                score += 2;
            }
            else if (6 <= numberOfPaths && numberOfPaths <= 10)
            {
                score += 1;
            }
            else if (11 <= numberOfPaths && numberOfPaths <= 20)
            {
                score += 0.5f;
            }
        
            return score;
        }
        private float PercentageOfChromosomeUsed()
        {
            int numberBricksPlaced = Utility.GetGameMap()[BlockType.FREE_TO_WALK].Count +
                                     Utility.GetGameMap()[BlockType.BASIC_BLOCK].Count;
            
            return numberBricksPlaced / (float) (Constants.CHROMOSONE_LENGTH * 3);
        }
        private float PercentageOfChromosomeThatIsNullSpace()
        {
            int numberOfIntentionalNullSpace = Utility.GetGameMap()[BlockType.NONE].Count;

            float used = numberOfIntentionalNullSpace / (float) (Constants.CHROMOSONE_LENGTH * 3);
            return used;
        }
        private float CalculateVolumeUsed()
        {
            float score = 0;
            int numberBricksPlaced = Utility.GetGameMap()[BlockType.FREE_TO_WALK].Count +
                                     Utility.GetGameMap()[BlockType.BASIC_BLOCK].Count;

            float used = numberBricksPlaced / (float) Constants.TOTAL_VOLUMNE;
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
        private float PercentageOfWalkableSurfaces()
        {
            int numberBricksPlaced = Utility.GetGameMap()[BlockType.FREE_TO_WALK].Count +
                                     Utility.GetGameMap()[BlockType.BASIC_BLOCK].Count;
            
            return Utility.GetGameMap()[BlockType.FREE_TO_WALK].Count / (float) numberBricksPlaced;;
        }

        private float CalculateLinearity()
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

            return (diffValues * 1.0f) / allBricks.Count;
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

            if (!_ga.playedLevels.Contains(chromosome))
            {
                _ga.playedLevels.Add(chromosome);
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
                
                Constants.CHROMOSONE_LENGTH = 5;
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
                        ((GaImplementation)targ).Remove();
                    }
                }
            }
        }

#endif
    }
}
