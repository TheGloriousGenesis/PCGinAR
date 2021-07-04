using System.Collections.Generic;
using UnityEngine;
using BasicGeneticAlgorithmNS;
using BaseGeneticClass;
using System.Linq;
using System;
using UnityEngine.AI;

public class PlatformGenerator : MonoBehaviour
{

    #region Platform assets
    public PrefabFactory prefabs;

    public BlockTile[] blockTiles;
    #endregion

    //private BasicGeneticAlgorithm bga = new BasicGeneticAlgorithm();

    //public NavMeshSurface surface;

    public void CreatePlatform(Quaternion orientation, Chromosone chromosone)
    {
        PlacePlatform(orientation, chromosone);
        ObtainWalkableSurface();
    }

    private void PlacePlatform(Quaternion orientation, Chromosone chromosone)
    {
        Vector3 blockSize = prefabs[BlockType.BASICBLOCK].transform.localScale;

        List<Gene> genes = chromosone.genes;
        // select unique from list 
        List<Vector3> position = genes.SelectMany(x => x.allele.blockPositions).Distinct().ToList();

        foreach (Vector3 i in position)
        {
            GameObject block1 = Instantiate(prefabs[BlockType.BASICBLOCK], i, orientation);
            block1.transform.parent = this.transform;
        }
    }

    private void ObtainWalkableSurface()
    {
        GameObject platform = this.gameObject;
        HashSet<Vector3> surface = new HashSet<Vector3>();
        foreach (Transform child in platform.transform)
        {
            RaycastHit hit;

            if (!Physics.Raycast(child.transform.position, Vector3.up * 1.5f, out hit, 2))
            {
                surface.Add(child.transform.position);
            }
        }

        Utility.gamePlacement = surface.ToDictionary(x => x, x => BlockType.NONE);
    }

    public GameObject PlacePlayer(Quaternion rotation, BlockType playerType)
    {
        GameObject goal = GameObject.FindGameObjectWithTag("GoalPost");
        float maxDistance = 0f;
        Vector3 farthestBrick = new Vector3();

        foreach (Vector3 child in Utility.gamePlacement.Keys)
        {
            //Debug.DrawRay(goal.transform.position, child, Color.white, 20);

            float currentDistance = Vector3.Distance(goal.transform.position, child);

            if (currentDistance > maxDistance)
            {
                maxDistance = currentDistance;
                farthestBrick = child;
            }
        }

        //Debug.DrawRay(goal.transform.position, farthestBrick, Color.green, 30);

        GameObject player;
        if (playerType == BlockType.AGENT)
        {
            player = GameObject.FindGameObjectWithTag("Agent");
            if (player == null)
            {
                player = Instantiate(prefabs[BlockType.PLAYER], farthestBrick + BlockPosition.UP * 1.1f, rotation);
            } else
            {
                player.gameObject.transform.position = farthestBrick + BlockPosition.UP * 1.1f;
                player.gameObject.transform.rotation = rotation;
            }

        }
        else
        {
            player = Instantiate(prefabs[BlockType.PLAYER], farthestBrick + BlockPosition.UP, rotation);
        }

        player.transform.parent = this.transform.parent;

        Utility.gamePlacement[farthestBrick] = BlockType.PLAYER;

        return player;
    }

    public void PlaceGoal(Vector3 position, Quaternion rotation)
    {
        Utility.gamePlacement[position] = BlockType.GOAL;

        GameObject goal_ = Instantiate(prefabs[BlockType.GOAL], position + Vector3.up * 1.1f, rotation);
        goal_.transform.parent = this.transform.parent;
    }
}
