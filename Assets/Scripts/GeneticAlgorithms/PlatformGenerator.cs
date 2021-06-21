using System.Collections.Generic;
using UnityEngine;
using BasicGeneticAlgorithmNS;
using BaseGeneticClass;
using System.Linq;
using System;
using UnityEngine.AI;

public class PlatformGenerator : MonoBehaviour
{
    public PrefabFactory prefabs;

    public BlockTile[] blockTiles;

    private BasicGeneticAlgorithm bga = new BasicGeneticAlgorithm();

    public NavMeshSurface[] surfaces;

    public void CreatePlatform(Quaternion orientation)
    {
        PlacePlatform(orientation);
        ObtainWalkableSurface();
        PlaceGoal(Utility.walkableSurface[0], orientation);
        PlacePlayer(orientation);
    }

    private void PlacePlatform(Quaternion orientation)
    {
        Chromosone chromosone = bga.GenerateChromosome();

        Vector3 blockSize = prefabs[BlockType.BASICBLOCK].transform.localScale;

        List<Gene> genes = chromosone.genes;
        List<Vector3> position = genes.SelectMany(x => x.allele.blockPositions).ToList();

        foreach (Vector3 i in position)
        {
            GameObject block1 = Instantiate(prefabs[BlockType.BASICBLOCK], i, orientation);
            block1.transform.parent = this.transform;
        }

        Debug.Log("surfaces to bake: " + surfaces.Length);
        for (int i = 0; i < surfaces.Length; i++)
        {
            surfaces[i].BuildNavMesh();
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
        Utility.walkableSurface = surface.ToList();
        Debug.Log("Walkable tiles: " + Utility.walkableSurface.Count);
    }

    private GameObject PlacePlayer(Quaternion rotation)
    {
        GameObject goal = GameObject.FindGameObjectWithTag("GoalPost");
        float maxDistance = 0f;
        Vector3 farthestBrick = new Vector3();

        foreach (Vector3 child in Utility.walkableSurface)
        {
            //Debug.DrawRay(goal.transform.position, child, Color.white, 20);

            float currentDistance = Vector3.Distance(goal.transform.position, child);

            if (currentDistance > maxDistance)
            {
                maxDistance = currentDistance;
                farthestBrick = child;
            }
        }
        Utility.currentAgentPosition = farthestBrick;

        //Debug.DrawRay(goal.transform.position, farthestBrick, Color.green, 30);

        GameObject player = Instantiate(prefabs[BlockType.AGENT], farthestBrick + BlockPosition.UP * 2, rotation);
        player.transform.parent = this.transform.parent;

        Utility.walkableSurface.Remove(farthestBrick);

        return player;
    }

    private void PlaceGoal(Vector3 position, Quaternion rotation)
    {
        Utility.walkableSurface.Remove(position);

        GameObject goal_ = Instantiate(prefabs[BlockType.GOAL], position + Vector3.up * 2, rotation);
        goal_.transform.parent = this.transform.parent;
    }
}
