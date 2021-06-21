using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.EventSystems;

[System.Serializable]
public static class Utility
{
    public static List<Vector3> walkableSurface = new List<Vector3>();
    public static Vector3 currentAgentPosition = new Vector3();
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
public static class BlockUIExtensions
{
    // checks to see if position is over ui/ gameobject
    public static bool IsPointOverUIObject(this Vector2 pos)
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }

        PointerEventData eventPostiion = new PointerEventData(EventSystem.current);
        eventPostiion.position = new Vector2(pos.x, pos.y);

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventPostiion, results);

        return results.Count > 0;
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
    ENEMY_2,
    AGENT
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


