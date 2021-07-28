using System.Collections.Generic;
using System.Linq;
using Behaviour.Entities;
using UnityEngine;
using Utilities;

namespace GeneticAlgorithms.Generators
{
    public class CoinGenerator : MonoBehaviour
    {
        public PrefabFactory prefabs;

        public void PlaceCoins()
        {
            PlaceCoins(Utility.GamePlacement);
        }

        private void PlaceCoins(Dictionary<Vector3, BlockType> gamePlacement)
        {
            var tmp = gamePlacement.ToList();

            foreach (var i in tmp)
            {
                if (i.Value != BlockType.NONE) continue;
                var coin = Instantiate(prefabs[BlockType.COIN], i.Key + Vector3.up * 2, Quaternion.identity);
                coin.transform.parent = transform;
                Utility.GamePlacement[i.Key] = BlockType.COIN;
            }
        }
    }
}
