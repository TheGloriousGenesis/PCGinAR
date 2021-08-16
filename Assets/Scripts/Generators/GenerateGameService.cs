using System;
using System.Collections.Generic;
using System.Linq;
using Behaviour.Entities;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Parameter;
using UI;
using UnityEngine;
using Utilities;

namespace Generators
{
    public class GenerateGameService : MonoBehaviour
    {
        private PlatformGenerator _platformGenerator;
        private CoinGenerator _coinGenerator;

        [SerializeField] 
        private GameObject game;

        private void Awake()
        {
            RetrieveComponents();
        }
        
        #region In Play Mode
        public GameObject CreateGameInPlay(Vector3 plane, Quaternion rotation, Chromosome chromosome)
        {
            CreateGame(rotation, Constants.PLAYERTYPE, chromosome);
            ConfigureGameSpace(plane);
            EventManager.current.CurrentChromosomeInPlay(chromosome.ID);
            return game;
        }    
    
        private void CreateGame(Quaternion orientation, BlockType playerType, Chromosome chromosome)
        {
            ResetGame(Utility.SafeDestroyInPlayMode);

            _platformGenerator.CreatePlatform(orientation, chromosome);
        
            _platformGenerator.CreateLinks();
        
            _platformGenerator.PlaceGoal(orientation);
        
            _platformGenerator.PlacePlayer(orientation, playerType);
            
            _coinGenerator.PlaceCoins(6);
        }

        private void ConfigureGameSpace(Vector3 plane)
        {
            game.transform.position = plane;
        }

        #endregion
    
        #region In GA Mode
    
        public GameObject CreateGameGA(BlockType playerType, Chromosome chromosome)
        {
            return CreateGameGA(Quaternion.identity, playerType, chromosome);
        }

        private GameObject CreateGameGA(Quaternion orientation, BlockType playerType, Chromosome chromosome)
        {
            if (_platformGenerator == null)
                _platformGenerator = GetComponentInChildren<PlatformGenerator>();
            if (_coinGenerator == null)
                _coinGenerator = GetComponentInChildren<CoinGenerator>(); 

            ResetGame(Utility.SafeDestroyInEditMode);

            _platformGenerator.CreatePlatform(orientation, chromosome);

            _platformGenerator.CreateLinks();

            _platformGenerator.PlaceGoal(orientation);
            _platformGenerator.PlacePlayer(orientation, playerType);
        
            return GameObject.Find("/GAME");
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
            
            if (!_platformGenerator)
                RetrieveComponents();
            _platformGenerator.DestoryLinksAndSurfaceNavMesh();
            
            // game.transform.position = new Vector3(0, 0, 0);
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
