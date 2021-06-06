using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[System.Serializable]
public static class Utility
{
    public static T DeepClone<T>(this T obj)
    {
        using (var ms = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;

            return (T)formatter.Deserialize(ms);
        }
    }

    // Inspired from https://forum.unity.com/threads/so-why-is-destroyimmediate-not-recommended.526939/
    public static void SafeDestory(GameObject obj)
    {
        obj.transform.parent = null;
        obj.name = "$disposed";
        UnityEngine.Object.Destroy(obj);
        obj.SetActive(false);
    }

}

[System.Serializable]
public enum BlockType
{
    BASICBLOCK,
    GOAL,
    PLAYER,
    COIN,
    ENEMY_1,
    ENEMY_2
}

[System.Serializable]
public class BlockTile
{
    public BlockType blockType;
    public float perlinNoiseLowLimit;
    public float perlinNoiseHighLimit;
}

[System.Serializable]
public static class BlockPosition
{
    public static readonly Vector3 UP = new Vector3(0, Constants.BLOCK_SIZE, 0);
    public static readonly Vector3 DOWN = new Vector3(0, -Constants.BLOCK_SIZE, 0);
    public static readonly Vector3 LEFT = new Vector3(-Constants.BLOCK_SIZE, 0, 0);
    public static readonly Vector3 RIGHT = new Vector3(Constants.BLOCK_SIZE, 0, 0);
    public static readonly Vector3 FRONT = new Vector3(0, 0, Constants.BLOCK_SIZE);
    public static readonly Vector3 BACK = new Vector3(0, 0, -Constants.BLOCK_SIZE);
    public static readonly Vector3 NONE = new Vector3(0, 0, 0);
}

[System.Serializable]
public struct SerializeVector3
{
    public float x;
    public float y;
    public float z;

    public SerializeVector3(float paramX, float paramY, float paramZ)
    {
        x = paramX;
        y = paramY;
        z = paramZ;
    }
}


