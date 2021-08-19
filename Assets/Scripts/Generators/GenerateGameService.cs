using System;
using System.Collections.Generic;
using System.Diagnostics;
using Behaviour.Entities;
using GeneticAlgorithms.Algorithms;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Parameter;
using UI;
using UnityEngine;
using Utilities;
using Debug = UnityEngine.Debug;

namespace Generators
{
    public class GenerateGameService : MonoBehaviour
    {
        private PlatformGenerator _platformGenerator;
        private CoinGenerator _coinGenerator;

        [SerializeField] 
        private GameObject game;

        private Stopwatch stop = new Stopwatch();

        private void Awake()
        {
            RetrieveComponents();
        }
        
        #region In Play Mode
        public GameObject CreateGame(Vector3 plane, Quaternion rotation, Chromosome chromosome, BlockType player)
        {
#if UNITY_EDITOR
            if (_platformGenerator == null)
                _platformGenerator = GetComponentInChildren<PlatformGenerator>();
            if (_coinGenerator == null)
                _coinGenerator = GetComponentInChildren<CoinGenerator>();
#endif
            CreateGame(rotation, player, chromosome);
#if !UNITY_EDITOR
            ConfigureGameSpace(plane);
            EventManager.current.CurrentChromosomeInPlay(chromosome.ID);
#endif
            return game;
        }    
    
        private void CreateGame(Quaternion orientation, BlockType playerType, Chromosome chromosome)
        {
#if UNITY_EDITOR
            ResetGame(Utility.SafeDestroyInEditMode);
#else 
            ResetGame(Utility.SafeDestroyInPlayMode);
#endif
            _platformGenerator.CreatePlatform(orientation, chromosome);
        
            // stop.Reset();
            // stop.Start();
            _platformGenerator.CreateLinks();
            // stop.Stop();
            // Debug.Log($"LINK generartion time: {stop.Elapsed.Milliseconds}");
            _platformGenerator.PlaceGoal(orientation);
        
            _platformGenerator.PlacePlayer(orientation, playerType);
        }

        private void ConfigureGameSpace(Vector3 plane)
        {
            game.transform.position = plane;
        }

        #endregion

        #region Delete methods
        public void ResetGame(Action<GameObject> delFunc)
        {
            DestroyPlatform(delFunc);

            DestroyCoins(delFunc);

            DestroyGoal(delFunc);

            DestroyPlayer(delFunc);

            DestroyAgent(delFunc);

            DestroyGamePlacement();

#if UNITY_EDITOR
            RetrieveComponents();
#endif
            _platformGenerator.DestoryLinksAndSurfaceNavMesh();
        }
    
        private static void DestroyGamePlacement()
        {
            Utility.ResetGameMap();
        }

        private static void DestroyPlatform(Action<GameObject> delFunc)
        {
            var bricks = GameObject.FindGameObjectsWithTag("Brick");
            if (bricks == null) return;
            for (var i = bricks.Length - 1; i >= 0; i--)
            {
                delFunc(bricks[i].gameObject);
            }
        }

        public static void DestroyCoins(Action<GameObject> delFunc)
        {
            var coins = GameObject.FindGameObjectsWithTag("Coin");
            if (coins != null)
            {
                for (var i = coins.Length - 1; i >= 0; i--)
                {
                    delFunc(coins[i].gameObject);
                }
            }
            Utility.GetGameMap()[BlockType.COIN] = new List<Vector3>();
        }

        private static void DestroyGoal(Action<GameObject> delFunc)
        {
            GameObject goal = GameObject.FindGameObjectWithTag("Goal");

            if (goal != null)
            {
                delFunc(goal);
            }
            Utility.GetGameMap()[BlockType.GOAL] = new List<Vector3>();
        }

        private static void DestroyPlayer(Action<GameObject> delFunc)
        {
            var player = GameObject.FindGameObjectsWithTag("Player");
            if (player != null)
            {
                for (var i = player.Length - 1; i >= 0; i--)
                {
                    delFunc(player[i].gameObject);
                }
            }
            Utility.GetGameMap()[BlockType.PLAYER] = new List<Vector3>();
        }
    
        private static void DestroyAgent(Action<GameObject> delFunc)
        {
            var player = GameObject.FindGameObjectsWithTag("Agent");
            if (player != null)
            {
                for (var i = player.Length - 1; i >= 0; i--)
                {
                    delFunc(player[i].gameObject);
                }
            }
            Utility.GetGameMap()[BlockType.AGENT] = new List<Vector3>();
        }
        #endregion

        public void PostProcessingAdjustments(int numberOfCoins)
        {
            _coinGenerator.PlaceCoins(numberOfCoins);
        }
        
        private void RetrieveComponents()
        {
            _platformGenerator = GetComponentInChildren<PlatformGenerator>();
            _coinGenerator = GetComponentInChildren<CoinGenerator>(); 
        }

        private void OnApplicationQuit()
        {
            ResetGame(Utility.SafeDestroyInEditMode);
        }
    }
}
