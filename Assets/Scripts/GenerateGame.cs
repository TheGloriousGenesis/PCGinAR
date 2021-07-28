using System.Collections.Generic;
using System.Linq;
using Behaviour.Entities;
using Generators;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Generators;
using LinkGenerator;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities;

public class GenerateGame : MonoBehaviour
{
    [SerializeField]
    private Text playerStats;

    [SerializeField]
    private PlatformGenerator platform;

    [SerializeField]
    private CoinGenerator coins;

    [FormerlySerializedAs("links_AutoPlacer")] [SerializeField]
    private NavMeshLinksAutoPlacer linksAutoPlacer;
    
    public GameObject CreateGame(BlockType playerType, Chromosome chromosome)
    {
        return CreateGame(new Vector3(), Quaternion.identity, playerType, chromosome);
    }

    private GameObject CreateGame(Vector3 plane, Quaternion orientation, BlockType playerType, Chromosome chromosome)
    {
        ResetGameArea();

        platform.CreatePlatform(orientation, chromosome);

        linksAutoPlacer.RefreshLinks();

        platform.PlaceGoal(Utility.GamePlacement.ElementAt(0).Key, orientation);
        platform.PlacePlayer(orientation, playerType);
        
        return ConfigureGameSpace(plane);
    }

    private GameObject ConfigureGameSpace(Vector3 plane)
    {
        GameObject game = GameObject.Find("/GAME");

        // This line might not be needed. Why dont i try placing object in front of camera using camera transformation.
        //game.transform.position = plane;
        //game.transform.rotation = Quaternion.Inverse(game.transform.rotation);

        // Might be able to set platform scale before hand. Maybe do a generic config file that sets scales and rotation for each asset attached?
        // have tried to rescale before brick added and that didnt work so think about it
        //GameObject.Find("/GAME").transform.localScale = GameObject.Find("/GAME").transform.localScale;
        return game;
    }

    public void ResetGameArea()
    {
        linksAutoPlacer.ClearLinks();

        DestoryPlatform();

        DestoryCoins();

        DestoryGoal();

        DestoryPlayer();

        DestoryAgent();

        DestoryGamePlacement();

        linksAutoPlacer.ClearSurfaceData();
    }
    
    private static void DestoryGamePlacement()
    {
        Utility.GamePlacement = new Dictionary<Vector3, BlockType>();
    }

    private static void DestoryPlatform()
    {
        var platform = GameObject.FindGameObjectsWithTag("Brick");
        if (platform == null) return;
        for (var i = platform.Length - 1; i >= 0; i--)
        {
            Utility.SafeDestoryInEditMode(platform[i].gameObject);
        }
    }

    public static void DestoryCoins()
    {
        var coins = GameObject.FindGameObjectsWithTag("Coin");
        if (coins != null)
        {
            for (var i = coins.Length - 1; i >= 0; i--)
            {
                Utility.SafeDestoryInEditMode(coins[i].gameObject);
            }
        }
        Utility.ReplaceValueInMap(Utility.GamePlacement, BlockType.COIN, BlockType.NONE);
    }

    private static void DestoryGoal()
    {
        GameObject goal = GameObject.FindGameObjectWithTag("Goal");

        if (goal != null)
        {
            Utility.SafeDestoryInEditMode(goal);
        }
        Utility.ReplaceValueInMap(Utility.GamePlacement, BlockType.GOAL, BlockType.NONE);
    }

    private static void DestoryPlayer()
    {
        var player = GameObject.FindGameObjectsWithTag("Player");
        if (player != null)
        {
            for (var i = player.Length - 1; i >= 0; i--)
            {
                Utility.SafeDestoryInEditMode(player[i].gameObject);
            }
        }
        Utility.ReplaceValueInMap(Utility.GamePlacement, BlockType.PLAYER, BlockType.NONE);
    }
    
    private static void DestoryAgent()
    {
        var player = GameObject.FindGameObjectsWithTag("Agent");
        if (player != null)
        {
            for (var i = player.Length - 1; i >= 0; i--)
            {
                Utility.SafeDestoryInEditMode(player[i].gameObject);
            }
        }
        Utility.ReplaceValueInMap(Utility.GamePlacement, BlockType.AGENT, BlockType.NONE);
    }

    public NavMeshPath PathStatus()
    {
        return NavMeshLinksAutoPlacer.ContainsPath();
    }
    public void PlaceCoins()
    {
        coins.PlaceCoins();
    }
}
