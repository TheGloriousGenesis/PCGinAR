using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Behaviour.Entities
{
    [CreateAssetMenu(fileName = "PrefabFactory", menuName = "Assets/Scripts")]
    public class PrefabFactory : ScriptableObject
    {
        [Serializable]
        public class BlockPrefab
        {
            public GameObject prefab;
            public BlockType blockType;
        }

        private Dictionary<BlockType, GameObject> _dict;
        [FormerlySerializedAs("AllPrefabs")] public BlockPrefab[] allPrefabs;

        public GameObject this[BlockType type]
        {
            get
            {
                Init();
                return _dict[type];
            }
        }

        private void Init()
        {
            if (_dict != null)
                return;
            _dict = new Dictionary<BlockType, GameObject>();
            foreach (var tile in allPrefabs)
            {
                _dict[tile.blockType] = tile.prefab;
            }
        }
    }
}
