using Behaviour.Entities;
using UnityEngine;
using Utilities;

public class PerlinNoiseExample : MonoBehaviour
{
    // Inspired by https://gamedev.stackexchange.com/questions/169192/select-unity-prefab-from-enum-choice
    [System.Serializable]
    public class BlockTile
    {
        public BlockType blockType;
        public float perlinNoiseLowLimit;
        public float perlinNoiseHighLimit;
    }

    public PrefabFactory prefabs;
    public BlockTile[] blockTiles;

    public int platformLength;
    public int platformHeight;

    private Random rand = new Random();
    private float[,] perlinMap;

    // I wish to store objects and their orientation in space in an object 
    // do I actually just have to store the space positions of these objections?
    // e.g can state type and then position = (Enum TYPE, float xPos, float yPos, float zPos)
    // we then instantiate from this list
    public void CreatePlatform()
    {
        for (int x = 0; x < platformHeight; x++)
        {
            GameObject newObj = Instantiate(prefabs[BlockType.BASIC_BLOCK], new Vector3(x, 0, 0), Quaternion.identity);
            newObj.transform.parent = GameObject.Find("Platform").transform;
        }
    }

    public void CreatePlatform(Vector3 startPosition, Quaternion startRotation)
    {
        int count = 0;
        while (count < platformHeight)
        {
            GameObject newObj = Instantiate(prefabs[BlockType.BASIC_BLOCK], startPosition + new Vector3(count, 0, 0), startRotation);
            newObj.transform.parent = GameObject.Find("Platform").transform;
        }
    }


}
