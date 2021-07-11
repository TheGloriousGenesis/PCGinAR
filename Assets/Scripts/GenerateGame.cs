using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using BaseGeneticClass;
using eDmitriyAssets.NavmeshLinksGenerator;
using UnityEngine.AI;

public class GenerateGame : MonoBehaviour
{
    [SerializeField]
    private Text playerStats;

    [SerializeField]
    private PlatformGenerator platform;

    [SerializeField]
    private CoinGenerator coins;

    [SerializeField]
    private NavMeshLinks_AutoPlacer links_AutoPlacer;

    private void Awake()
    {
        playerStats.text = "Coin counter: 0";
    }

    public GameObject CreateGame(BlockType playerType, Chromosome chromosome)
    {
        return CreateGame(new Vector3(), Quaternion.identity, playerType, chromosome);
    }

    public GameObject CreateGame(Vector3 plane, Quaternion orientation, BlockType playerType, Chromosome chromosome)
    {
        ResetGameArea();

        platform.CreatePlatform(orientation, chromosome);

        links_AutoPlacer.RefreshLinks();

        platform.PlaceGoal(Utility.gamePlacement.ElementAt(0).Key, orientation);
        platform.PlacePlayer(orientation, playerType);
        
        // PlaceCoins();
        
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
        links_AutoPlacer.ClearLinks();

        DestoryPlatform();

        DestoryCoins();

        DestoryGoal();

        DestoryPlayer();

        ResetGameMap();

        links_AutoPlacer.ClearSurfaceData();
    }

    public void ResetGameMap()
    {
        foreach( var key in Utility.gamePlacement.Keys.ToList())
        {
            Utility.gamePlacement[key] = BlockType.NONE;
        } 
    }

    public void DestoryGamePlacement()
    {
        Utility.gamePlacement = new Dictionary<Vector3, BlockType>();
    }

    public void DestoryPlatform()
    {
        GameObject[] platform = GameObject.FindGameObjectsWithTag("Brick");
        if (platform != null)
        {
            for (int i = platform.Length - 1; i >= 0; i--)
            {
                Utility.SafeDestoryInEditMode(platform[i].gameObject);
            }
        }
    }

    public void DestoryCoins()
    {
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        if (coins != null)
        {
            for (int i = coins.Length - 1; i >= 0; i--)
            {
                Utility.SafeDestoryInEditMode(coins[i].gameObject);
            }
        }
        Utility.ReplaceValueInMap(Utility.gamePlacement, BlockType.COIN, BlockType.NONE);
    }

    public void DestoryGoal()
    {
        GameObject goal = GameObject.FindGameObjectWithTag("Goal");

        if (goal != null)
        {
            Utility.SafeDestoryInEditMode(goal);
        }
        Utility.ReplaceValueInMap(Utility.gamePlacement, BlockType.GOAL, BlockType.NONE);
    }

    public void DestoryPlayer()
    {
        GameObject[] player = GameObject.FindGameObjectsWithTag("Player");
        if (player != null)
        {
            for (int i = player.Length - 1; i >= 0; i--)
            {
                Utility.SafeDestoryInEditMode(player[i].gameObject);
            }
        }
        Utility.ReplaceValueInMap(Utility.gamePlacement, BlockType.PLAYER, BlockType.NONE);
    }

    public NavMeshPath PathStatus()
    {
        return links_AutoPlacer.containsPath();
    }
    public void PlaceCoins()
    {
        coins.PlaceCoins();
    }
}
