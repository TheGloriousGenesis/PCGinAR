using System.Collections.Generic;
using System.Linq;
using Behaviour.Entities;
using GeneticAlgorithms.Entities;
using UnityEngine;
using Utilities;

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
            List<Vector3> position = genes.SelectMany(x => x.allele.blockPositions).Distinct().ToList();

            foreach (Vector3 i in position)
            {
                GameObject block1 = Instantiate(prefabs[BlockType.BASICBLOCK], i, orientation);
                block1.transform.parent = transform;
            }
        }

        private void ObtainWalkableSurface()
        {
            GameObject platform = gameObject;
            HashSet<Vector3> surface = new HashSet<Vector3>();
            foreach (Transform child in platform.transform)
            {
                if (!Physics.Raycast(child.transform.position, Vector3.up * 1.5f, out _, 2))
                {
                    surface.Add(child.transform.position);
                }
            }

            Utility.GamePlacement = surface.ToDictionary(x => x, x => BlockType.NONE);
        }

        public GameObject PlacePlayer(Quaternion rotation, BlockType playerType)
        {
            GameObject goal = GameObject.FindGameObjectWithTag("GoalPost");
            float maxDistance = 0f;
            Vector3 farthestBrick = new Vector3();

            foreach (Vector3 child in Utility.GamePlacement.Keys)
            {
                float currentDistance = Vector3.Distance(goal.transform.position, child);

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
                    player = Instantiate(prefabs[BlockType.AGENT], farthestBrick + BlockPosition.UP * 1.1f, rotation);
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

            player.transform.parent = transform.parent;

            Utility.GamePlacement[farthestBrick] = playerType;

            return player;
        }

        public void PlaceGoal(Vector3 position, Quaternion rotation)
        {
            // populate position in game map
            Utility.GamePlacement[position] = BlockType.GOAL;

            var goal = Instantiate(prefabs[BlockType.GOAL], position + Vector3.up * 1.1f, rotation);
            goal.transform.parent = this.transform.parent;
        }
    }
}
