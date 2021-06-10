using System.Collections.Generic;
using UnityEngine;
using BasicGeneticAlgorithmNS;
using BaseGeneticClass;
using System.Linq;
using System;

public class PlatformGenerator : MonoBehaviour
{
    public PrefabFactory prefabs;

    public BlockTile[] blockTiles;

    private BasicGeneticAlgorithm bga = new BasicGeneticAlgorithm();

    public void CreatePlatform(Vector3 plane, Quaternion orientation)
    {
        PlacePlatform(orientation);
        ObtainWalkableSurface();
        PlaceGoal(PlacePlayer(Utility.walkableSurface[0], orientation));
    }

    private void PlacePlatform(Quaternion orientation)
    {
        Chromosone chromosone = bga.GenerateChromosome(4);

        Vector3 blockSize = prefabs[BlockType.BASICBLOCK].transform.localScale;

        List<Gene> genes = chromosone.genes;
        List<Allele> position = genes.Select(x => x.allele).ToList();

        foreach (Allele i in position)
        {
            GameObject block1 = Instantiate(prefabs[BlockType.BASICBLOCK], new Vector3(i.blockPositions[0][0], i.blockPositions[0][1], i.blockPositions[0][2]), orientation);
            block1.transform.parent = this.transform;

            GameObject block2 = Instantiate(prefabs[BlockType.BASICBLOCK], block1.transform.position + i.blockPositions[1], orientation);
            block2.transform.parent = this.transform;

            GameObject block3 = Instantiate(prefabs[BlockType.BASICBLOCK], block2.transform.position + i.blockPositions[2], orientation);
            block3.transform.parent = this.transform;
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
    }

    private GameObject PlacePlayer(Vector3 position, Quaternion rotation)
    {
        GameObject player = Instantiate(prefabs[BlockType.AGENT], position + BlockPosition.UP * 8, rotation);
        player.transform.parent = this.transform.parent;
        Utility.walkableSurface.Remove(position);
        Utility.currentAgentPosition = position;
        return player;
    }

    private void PlaceGoal(GameObject player)
    {
        float maxDistance = 0f;
        Vector3 farthestBrick = new Vector3();

        foreach (Vector3 child in Utility.walkableSurface)
        {
            float currentDistance = Vector3.Distance(player.transform.position, child);

            //Debug.Log("Current Distance: " + currentDistance);
            if (currentDistance > maxDistance)
            {
                maxDistance = currentDistance;
                farthestBrick = child;
            }
        }

        GameObject goal_ = Instantiate(prefabs[BlockType.GOAL], farthestBrick + Vector3.up * 2, Quaternion.identity);
        goal_.transform.parent = this.transform.parent;
        Utility.walkableSurface.Remove(farthestBrick);
    }
}
