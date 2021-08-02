using System;
using System.Collections.Generic;
using System.Diagnostics;
using Behaviour.Entities;
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
        [Header("Game Property")] 
        [SerializeField]
        private GenerateGameService gameService;
        [FormerlySerializedAs("multiplePaths")] [SerializeField] PathFinding3D multiplePathsFinding;
    
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

        private void Start()
        {
            if (_random == null) _random = new Random(Constants.SEED);
            
            if (_ga == null) _ga = new BasicGeneticAlgorithm(Constants.POPULATION_SIZE, Constants.CHROMOSONE_LENGTH, Constants.CROSSOVER_PROBABILITY,
                _random, Constants.ELITISM, Constants.MUTATION_PROBABILITY, Constants.ITERATION, Constants.K);
        }

        public Chromosome Run()
        {
            List<Chromosome> finalResult = _ga.Run(FitnessFunction);
            return finalResult[1];
        }

        private void Remove()
        {
            gameService.ResetGame(Utility.SafeDestroyInEditMode);
            _ga = null;
            multiplePathsFinding.ResetPathFinding();
            Chromosome.ResetCounter();
            _random = null;
        }

        #region Fitness Analysis
        private double FitnessFunction(Chromosome chromosome)
        {
            Stopwatch timer = new Stopwatch();
            float score = 0;

            timer.Start();
            gameService.CreateGameGA(BlockType.AGENT, chromosome);
            timer.Stop();

            score += AnalyseAStarPath();
            score += AnalyseMultiPaths();

            score = (score + 0.5f)/(5 + 0.5f);
      
            chromosome.Fitness = score;
      
            return timer.Elapsed.TotalMilliseconds;
        }

        private float AnalyseMultiPaths()
        {
            // Check number of paths
            float score = 0;
            int numberOfPaths = CalculateNumberOfPaths();
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

        private float AnalyseAStarPath()
        {
            NavMeshPath path = GenerateGameService.PathStatus();
            float score = 0;
            // Check completable level
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                score += 1;
            }
        
            // Check Level usage
            if (path.GetTotalDistance() > Constants.MAX_PLATFORM_DIMENSION)
            {
                score += 1;
            }
            else
            {
                score -= 0.5f;
            }
        
            // todo: Have removed entropy calculate length in order to replace with ingame stats
            // // Check entropy
            // int numberOfTurns = path.corners.Length;
            // //todo: double check this 
            // // max number of turns is if a turn must be done on each block
            // int maxNumberOfTurns = Utility.GamePlacement.Keys.Count;
            // score += (numberOfTurns/maxNumberOfTurns);

            return score;
        }

        private int CalculateNumberOfPaths()
        {
            List<List<Vector3>> allPaths = multiplePathsFinding.FindPaths();
            return allPaths.Count;
        }
        
        #endregion

        #region InGame data for GA

        private void SetUpGARun(GameData gameData)
        {
            GameData.SaveGameData(gameData);
            var chromosome = _ga._currentPopulation.Find(x => x.ID == gameData.chromosomeID);
            if (!_ga.playedLevels.Contains(chromosome))
            {
                _ga.playedLevels.Add(chromosome);
            }
            Debug.Log($"Number of uniquely played levels: {_ga.playedLevels.Count}");
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
                        ((GaImplementation)targ).Run();
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
