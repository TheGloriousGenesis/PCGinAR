using Behaviour.Entities;
using UnityEngine;
using Utilities;

namespace Generators
{
    public class CoinGenerator : MonoBehaviour
    {
        public PrefabFactory prefabs;

        public void PlaceCoins()
        {
            var surface = Utility.GetGameMap()[BlockType.FREE_TO_WALK];

            // foreach (var i in surface)
            // {
            //     var coin = Instantiate(prefabs[BlockType.COIN], i + Vector3.up * 2, Quaternion.identity);
            //     coin.transform.parent = transform;
            //     Utility.GamePlacement[i.Key] = BlockType.COIN;
            // }
        }
    }
}
