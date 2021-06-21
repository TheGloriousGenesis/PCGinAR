using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinGenerator : MonoBehaviour
{
    public PrefabFactory prefabs;

    public void PlaceCoins()
    {
        PlaceCoins(Utility.walkableSurface);
    }

    public void PlaceCoins(List<Vector3> coins)
    {
        foreach (Vector3 i in coins)
        {
            GameObject coin = Instantiate(prefabs[BlockType.COIN], i + Vector3.up * 2, Quaternion.identity);
            coin.transform.parent = this.transform;
        }
    }
}
