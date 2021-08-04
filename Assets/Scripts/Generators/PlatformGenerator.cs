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
            List<BlockCube> position = genes.SelectMany(x => x.allele.blockPositions).Distinct().ToList();

            List<Vector3> nullSpace = new List<Vector3>();
            foreach (BlockCube i in position)
            {
                if (i.blockType == BlockType.NONE)
                {
                    nullSpace.Add(i.position);
                    continue;
                }
                GameObject block1 = Instantiate(prefabs[i.blockType], i.position, orientation);
                block1.transform.parent = transform;
            }
            Utility.GetGameMap()[BlockType.BASIC_BLOCK] = position.Where(x => x.blockType == BlockType.BASIC_BLOCK)
                .Select(x => x.position).ToList();
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

        public GameObject PlacePlayer(Quaternion rotation, BlockType playerType)
        {
            Vector3 goal = Utility.GetGameMap()[BlockType.GOAL][0];
            float maxDistance = 0f;
            Vector3 farthestBrick = new Vector3();
            
            // Debug.Log($" is goal null? {goal == null}");
            // iterate through each walkable surface
            foreach (Vector3 child in Utility.GetGameMap()[BlockType.FREE_TO_WALK])
            {
                float currentDistance = Vector3.Distance(goal, child);

                if (currentDistance > maxDistance)
                {
                    maxDistance = currentDistance;
                    farthestBrick = child;
                }
            }
            // NavMeshHit closestHit;
            GameObject player;
            // if (!NavMesh.SamplePosition(farthestBrick, out closestHit, 500, NavMesh.AllAreas)) return;
            
            if (playerType == BlockType.AGENT)
            {
                player = GameObject.FindGameObjectWithTag("Agent");
                if (player == null)
                {
                    player = Instantiate(prefabs[playerType], farthestBrick,
                        rotation);
                }
                else
                {
                    player.gameObject.transform.position = farthestBrick;
                    player.gameObject.transform.rotation = rotation;
                }
            }
            else
            {
                player = Instantiate(prefabs[playerType], farthestBrick + Vector3.up*2, rotation);
            }
            
            Utility.GetGameMap()[playerType] = new List<Vector3>() {farthestBrick};

            player.transform.parent = transform.parent;

            return player;
        }

        public void PlaceGoal(Quaternion rotation)
        {
            // populate position in game map randomly
            Vector3 goalPosition = Utility.GetKRandomElements(Utility.GetGameMap()[BlockType.FREE_TO_WALK], 1, 
                new Random(Constants.SEED))[0];
            
            Utility.GetGameMap()[BlockType.GOAL] = new List<Vector3>(){goalPosition};
            var goal = Instantiate(prefabs[BlockType.GOAL], goalPosition + Vector3.up * 1.12f, rotation);
            goal.transform.parent = this.transform.parent;
            // NavMeshHit closestHit;
            // if (NavMesh.SamplePosition(goalPosition, out closestHit, 500, NavMesh.AllAreas))
            // {
            //
            // }

        }
    }
}
