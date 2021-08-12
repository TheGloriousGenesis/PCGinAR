using System;
using System.Collections.Generic;
using System.Linq;
using Behaviour.Entities;
using GeneticAlgorithms.Entities;
using GeneticAlgorithms.Parameter;
using UnityEngine;
using UnityEngine.AI;
using Utilities;
using Random = System.Random;

namespace Generators
{
    public class PlatformGenerator : MonoBehaviour
    {

        #region Platform assets
        public PrefabFactory prefabs;
        #endregion

        public void CreatePlatform(Quaternion orientation, Chromosome chromosome)
        {
            PlacePlatform(orientation, chromosome);
            ObtainWalkableSurface();
        }

        private void PlacePlatform(Quaternion orientation, Chromosome chromosome)
        {
            List<Gene> genes = chromosome.Genes;
            // select unique from list 
            List<Allele> bricks = genes.Select(x => x.allele).ToList();
            
            List<Vector3> nullSpace = new List<Vector3>();
            List<Vector3> fullSpace = new List<Vector3>();
            
            foreach (Allele i in bricks)
            {
                if (i.chunkID == 15)
                {
                    nullSpace.Add(i.position);
                }
                
                List<Vector3> tmp = GetPosition(i);
                
                fullSpace.AddRange(tmp);
            }

            fullSpace = fullSpace.Distinct().ToList();
            nullSpace = nullSpace.Distinct().ToList();
            
            foreach(Vector3 v in fullSpace)
            {
                GameObject block1 = Instantiate(prefabs[BlockType.BASIC_BLOCK], v, orientation);
                block1.transform.parent = transform;
            }

            Utility.GetGameMap()[BlockType.BASIC_BLOCK] = fullSpace;
            Utility.GetGameMap()[BlockType.NONE] = nullSpace;
        }

        private void ObtainWalkableSurface()
        {
            GameObject platform = gameObject;
            HashSet<Vector3> surface = new HashSet<Vector3>();
            foreach (Transform child in platform.transform)
            {
                // check if there is anything above the brick
                if (!Physics.Raycast(child.transform.position, Vector3.up * 1.5f, out _, 2))
                {
                    surface.Add(child.transform.position);
                }
            }
            // get all bricks in game. Remove bricks that are walkable from basic block
            List<Vector3> tmp = Utility.GetGameMap()[BlockType.BASIC_BLOCK];
            tmp.RemoveAll(x => surface.Contains(x));
            Utility.GetGameMap()[BlockType.BASIC_BLOCK] = tmp;
            // add walkable blocks to map
            Utility.GetGameMap()[BlockType.FREE_TO_WALK] = surface.ToList();
        }

        public void PlacePlayer(Quaternion rotation, BlockType playerType)
        {
            Vector3 goal = Utility.GetGameMap()[BlockType.GOAL][0];
            float maxDistance = 0f;
            Vector3 farthestBrick = new Vector3();
            
            foreach (Vector3 child in Utility.GetGameMap()[BlockType.FREE_TO_WALK])
            {
                float currentDistance = Vector3.Distance(goal, child);

                if (currentDistance > maxDistance)
                {
                    maxDistance = currentDistance;
                    farthestBrick = child;
                }
            }
            
            GameObject player;
            
            if (playerType == BlockType.AGENT)
            {
                player = GameObject.FindGameObjectWithTag("Agent");

                if (player == null)
                {
                    NavMeshHit closestHit;
                    if( NavMesh.SamplePosition(farthestBrick, out closestHit, float.PositiveInfinity, NavMesh.AllAreas) ){
                        
                        player = Instantiate(prefabs[playerType], closestHit.position,
                            rotation);
                    }
                    else
                    {
                        Debug.Log("Can not find closest place on Mesh");
                        player = Instantiate(prefabs[playerType], farthestBrick,
                            rotation);
                    }
                }
                else
                {
                    player.gameObject.transform.position = farthestBrick;
                    player.gameObject.transform.rotation = rotation;
                }
            }
            else
            {
                player = Instantiate(prefabs[playerType], farthestBrick + Vector3.up, rotation);
            }
            
            Utility.GetGameMap()[playerType] = new List<Vector3>() {farthestBrick};

            player.transform.parent = transform.parent;
        }

        public void PlaceGoal(Quaternion rotation)
        {
            // populate position in game map randomly
            Vector3 goalPosition = Utility.GetKRandomElements(Utility.GetGameMap()[BlockType.FREE_TO_WALK], 1, 
                new Random(Constants.SEED))[0];
            
            Utility.GetGameMap()[BlockType.GOAL] = new List<Vector3>(){goalPosition};
            
            NavMeshHit closestHit;
            if( NavMesh.SamplePosition(goalPosition, out closestHit, float.PositiveInfinity, NavMesh.AllAreas) ){
                var goal = Instantiate(prefabs[BlockType.GOAL], closestHit.position, rotation);
                goal.transform.parent = this.transform.parent;
            }
            else
            {
                var goal = Instantiate(prefabs[BlockType.GOAL], goalPosition + Vector3.up * (Constants.BLOCK_SIZE) , rotation);
                goal.transform.parent = this.transform.parent;
            }
        }
        
        private List<Vector3> GetPosition(Allele allele)
        {
            List<Vector3> cubePosition = new List<Vector3>();
            switch (allele.chunkID)
            {
                case 0:
                    cubePosition.Add(allele.position);
                    cubePosition.Add(allele.position + BlockPosition.UP);
                    cubePosition.Add(allele.position + BlockPosition.RIGHT);
                    return cubePosition;
                case 1:
                    cubePosition.Add(allele.position);
                    cubePosition.Add(allele.position + BlockPosition.UP);
                    cubePosition.Add(allele.position + BlockPosition.LEFT);
                    return cubePosition;
                case 2:
                    cubePosition.Add(allele.position);
                    cubePosition.Add(allele.position + BlockPosition.DOWN);
                    cubePosition.Add(allele.position + BlockPosition.RIGHT);
                    return cubePosition;
                case 3:
                    cubePosition.Add(allele.position);
                    cubePosition.Add(allele.position + BlockPosition.DOWN);
                    cubePosition.Add(allele.position + BlockPosition.LEFT);
                    return cubePosition;
                case 4:
                    cubePosition.Add(allele.position);
                    cubePosition.Add(allele.position + BlockPosition.UP);
                    cubePosition.Add(allele.position + BlockPosition.FRONT);
                    return cubePosition;
                case 5:
                    cubePosition.Add(allele.position);
                    cubePosition.Add(allele.position + BlockPosition.FRONT);
                    cubePosition.Add(allele.position + BlockPosition.LEFT);
                    return cubePosition;
                case 6:
                    cubePosition.Add(allele.position);
                    cubePosition.Add(allele.position + BlockPosition.FRONT);
                    cubePosition.Add(allele.position + BlockPosition.RIGHT);
                    return cubePosition;
                case 7:
                    cubePosition.Add(allele.position);
                    cubePosition.Add(allele.position + BlockPosition.BACK);
                    cubePosition.Add(allele.position + BlockPosition.DOWN);
                    return cubePosition;
                case 8:
                    cubePosition.Add(allele.position);
                    cubePosition.Add(allele.position + BlockPosition.FRONT);
                    cubePosition.Add(allele.position + BlockPosition.DOWN);
                    return cubePosition;
                case 9:
                    cubePosition.Add(allele.position);
                    cubePosition.Add(allele.position + BlockPosition.UP);
                    cubePosition.Add(allele.position + BlockPosition.BACK);
                    return cubePosition;
                case 10:
                    cubePosition.Add(allele.position);
                    cubePosition.Add(allele.position + BlockPosition.RIGHT);
                    cubePosition.Add(allele.position + BlockPosition.BACK);
                    return cubePosition;
                case 11:
                    cubePosition.Add(allele.position);
                    cubePosition.Add(allele.position + BlockPosition.LEFT);
                    cubePosition.Add(allele.position + BlockPosition.BACK);
                    return cubePosition;
                case 12:
                    cubePosition.Add(allele.position);
                    cubePosition.Add(allele.position + BlockPosition.UP);
                    cubePosition.Add(allele.position + BlockPosition.DOWN);
                    return cubePosition;
                case 13:
                    cubePosition.Add(allele.position);
                    cubePosition.Add(allele.position + BlockPosition.LEFT);
                    cubePosition.Add(allele.position + BlockPosition.RIGHT);
                    return cubePosition;
                case 14:
                    cubePosition.Add(allele.position);
                    cubePosition.Add(allele.position + BlockPosition.FRONT);
                    cubePosition.Add(allele.position + BlockPosition.BACK);
                    return cubePosition;
                case 15:
                    return cubePosition;
            }

            return cubePosition;
        }

    }
}
