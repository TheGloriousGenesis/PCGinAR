using System;
using System.Collections.Generic;
using Behaviour.Entities;
using Behaviour.Platform.LinkGenerator;
using Generators;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Parameter;
using PathFinding.LinkGenerator;
using UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Utilities;

public class GenerateGameService : MonoBehaviour
{
    [FormerlySerializedAs("_platform")] [SerializeField]
    private PlatformGenerator platform;
    [FormerlySerializedAs("_coins")] [SerializeField]
    private CoinGenerator coins;
    [FormerlySerializedAs("_linksAutoPlacer")] [SerializeField]
    private NavMeshLinksAutoPlacer linksAutoPlacer;
    [SerializeField]
    private GenerateNavLinks gen;

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

        platform.CreatePlatform(orientation, chromosome);
        
        // todo: uncomment when adding enemies
        // linksAutoPlacer.RefreshLinks();

        // gen.DoGenerateLinks();
        
        platform.PlaceGoal(orientation);
        
        platform.PlacePlayer(orientation, playerType);
        
        return ConfigureGameSpace(plane);
    }

    private GameObject ConfigureGameSpace(Vector3 plane)
    {
        GameObject game = GameObject.Find("/GAME");
        game.transform.position = plane;
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

        platform.CreatePlatform(orientation, chromosome);

        linksAutoPlacer.RefreshLinks();

        platform.PlaceGoal(orientation);
        platform.PlacePlayer(orientation, playerType);
        
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
        gen.ClearLinks();
        
        linksAutoPlacer.ClearLinks();

        DestroyPlatform(delFunc);

        DestroyCoins(delFunc);

        DestroyGoal(delFunc);

        DestroyPlayer(delFunc);

        DestroyAgent(delFunc);

        DestroyGamePlacement();

        linksAutoPlacer.ClearSurfaceData();
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
