using Behaviour.Entities;
using UnityEngine;
using Utilities;
using Random = System.Random;

namespace Generators
{
    public class CoinGenerator : MonoBehaviour
    {
        public PrefabFactory prefabs;

        public void PlaceCoins(int numberOfCoins)
        {
            var surface = Utility.GetGameMap()[BlockType.FREE_TO_WALK];

            if (numberOfCoins > surface.Count)
                return;
            var placedPositions = Utility.GetKRandomElements(surface, numberOfCoins, new Random());
            
            foreach (var i in placedPositions)
            {
                var coin = Instantiate(prefabs[BlockType.COIN], i + Vector3.up, Quaternion.identity);
                coin.transform.parent = transform;
            }

            surface.RemoveAll(x => placedPositions.Contains(x));
            Utility.GetGameMap()[BlockType.FREE_TO_WALK] = surface;
            Utility.GetGameMap()[BlockType.COIN] = placedPositions;
        }
    }
}
