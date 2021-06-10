using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinGenerator : MonoBehaviour
{
    public PrefabFactory prefabs;

    public void PlaceCoins()
    {
        foreach (Vector3 i in Utility.walkableSurface)
        {
            GameObject coin = Instantiate(prefabs[BlockType.COIN], i + Vector3.up * 2, Quaternion.identity);
            coin.transform.parent = this.transform;
        }
    }
}
