using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CoinGenerator : MonoBehaviour
{
    public PrefabFactory prefabs;

    public void PlaceCoins()
    {
        PlaceCoins(Utility.gamePlacement);
    }

    public void PlaceCoins(Dictionary<Vector3, BlockType> gamePlacement)
    {
        var tmp = gamePlacement.ToList();

        foreach (KeyValuePair<Vector3,BlockType> i in tmp)
        {
            if (i.Value == BlockType.NONE)
            {
                GameObject coin = Instantiate(prefabs[BlockType.COIN], i.Key + Vector3.up * 2, Quaternion.identity);
                coin.transform.parent = this.transform;
                Utility.gamePlacement[i.Key] = BlockType.COIN;
            }
        }
    }
}
