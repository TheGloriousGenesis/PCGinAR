using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PrefabFactory", menuName = "Assets/Scripts")]
public class PrefabFactory : ScriptableObject
{
    [System.Serializable]
    public class BlockPrefab
    {
        public GameObject prefab;
        public BlockType blockType;
    }

    private Dictionary<BlockType, GameObject> dict;
    public BlockPrefab[] AllPrefabs;

    public GameObject this[BlockType type]
    {
        get
        {
            Init();
            return dict[type];
        }
    }

    private void Init()
    {
        if (dict != null)
            return;
        dict = new Dictionary<BlockType, GameObject>();
        foreach (var tile in AllPrefabs)
        {
            dict[tile.blockType] = tile.prefab;
        }
    }
}
