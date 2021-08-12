using System;
using System.Collections.Generic;
using Behaviour.Entities;
using Behaviour.Platform.LinkGenerator;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Parameter;
using PathFinding.LinkGenerator;
using UI;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

namespace Generators
{
    public class GenerateGameService : MonoBehaviour
    {
        private PlatformGenerator _platformGenerator;
        private CoinGenerator _coinGenerator;
        private NavMeshLinksAutoPlacer _linksAutoPlacer;
        // private GenerateNavLinks _gen;

        private void Awake()
        {
            _platformGenerator = GetComponentInChildren<PlatformGenerator>();
            _coinGenerator = GetComponentInChildren<CoinGenerator>();
            _linksAutoPlacer = GetComponent<NavMeshLinksAutoPlacer>();
            
        }

        #region In Play Mode
        public GameObject CreateGameInPlay(Vector3 plane, Quaternion rotation, Chromosome chromosome)
        {
            GameObject go = CreateGame(plane, rotation, Constants.PLAYERTYPE, chromosome);
            EventManager.current.CurrentChromosomeInPlay(chromosome.ID);
            return go;
        }    
    
        private GameObject CreateGame(Vector3 plane, Quaternion orientation, BlockType playerType, Chromosome chromosome)
        {
            ResetGame(Utility.SafeDestroyInPlayMode);

            _platformGenerator.CreatePlatform(orientation, chromosome);
        
            // todo: uncomment when adding enemies
            _linksAutoPlacer.RefreshLinks();

            // gen.DoGenerateLinks();
        
            _platformGenerator.PlaceGoal(orientation);
        
            _platformGenerator.PlacePlayer(orientation, playerType);
        
            return ConfigureGameSpace(plane);
        }

        private GameObject ConfigureGameSpace(Vector3 plane)
        {
            GameObject game = GameObject.Find("/GAME");
            // game.transform.position = plane;
            return game;
        }
    
        #endregion
    
        #region In GA Mode
    
        public GameObject CreateGameGA(BlockType playerType, Chromosome chromosome)
        {
            return CreateGameGA(Quaternion.identity, playerType, chromosome);
        }

        private GameObject CreateGameGA(Quaternion orientation, BlockType playerType, Chromosome chromosome)
        {
            ResetGame(Utility.SafeDestroyInEditMode);

            _platformGenerator.CreatePlatform(orientation, chromosome);

            _linksAutoPlacer.RefreshLinks();

            _platformGenerator.PlaceGoal(orientation);
            _platformGenerator.PlacePlayer(orientation, playerType);
        
            return GameObject.Find("/GAME");
        }
    
        public static NavMeshPath PathStatus()
        {
            return NavMeshLinksAutoPlacer.ContainsPath();
        }
        #endregion

        #region Delete methods
        public void ResetGame(Action<GameObject> delFunc)
        {
            // _gen.ClearLinks();
        
            _linksAutoPlacer.ClearLinks();

            DestroyPlatform(delFunc);

            DestroyCoins(delFunc);

            DestroyGoal(delFunc);

            DestroyPlayer(delFunc);

            DestroyAgent(delFunc);

            DestroyGamePlacement();

            _linksAutoPlacer.ClearSurfaceData();
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

        private void OnApplicationQuit()
        {
            ResetGame(Utility.SafeDestroyInEditMode);
        }
    }
}
