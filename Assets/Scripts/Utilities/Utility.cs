using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using BaseGeneticClass;
using UnityEngine.EventSystems;

[System.Serializable]
public static class Utility
{
    public static List<Vector3> walkableSurface = new List<Vector3>();

    public static Dictionary<Vector3, BlockType> gamePlacement = new Dictionary<Vector3, BlockType>();
    //public static Vector3 currentAgentPosition = new Vector3();
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

    // public static int[] GetRandomElements(int listLength, int elementsCount, System.Random random)
    // {
    //     return Enumerable.Range(0, listLength).OrderBy(x => random.Next())
    //         .Take(elementsCount).ToArray();
    // }

    public static List<T> GetKRandomElements<T>(List<T> list, int k, System.Random random)
    {
        return list.OrderBy(x => random.Next()).Take(k).ToList();
    }
    
    public static List<Chromosome> FindNBestFitness_ByChromosome(List<Chromosome> list, int n)
    {
        list.Sort(CompareChromosome);
        if (n == 1)
        {
            return new List<Chromosome>(){list[0]};
        }
        return list.Take(n) as List<Chromosome>;
    }
    
    public static void SafeDestoryInEditMode(GameObject obj)
    {
        obj.transform.parent = null;
        obj.name = "$disposed";
        UnityEngine.Object.DestroyImmediate(obj);
        //obj.SetActive(false);
    }

    public static List<Vector3> GetKeyFromValue(Dictionary<Vector3, BlockType> dic, BlockType value)
    {
        List<Vector3> allPositions = new List<Vector3>();

        foreach(KeyValuePair<Vector3, BlockType> i in dic)
        {
            if (i.Value == value)
            {
                allPositions.Add(i.Key);
            }
        }

        return allPositions;
    }

    public static void ReplaceValueInMap(Dictionary<Vector3, BlockType> dic, BlockType replace, BlockType value)
    {
        List<Vector3> replacableKeys = new List<Vector3>();

        foreach(KeyValuePair<Vector3, BlockType> i in dic)
        {
            if (i.Value == replace)
            {
                replacableKeys.Add(i.Key);
            }
        }
        
        foreach(Vector3 i in replacableKeys)
        {
            dic[i] = value;
        }
    }
    
    public static int CompareChromosome(Chromosome a, Chromosome b)
    {
        if (a.fitness > b.fitness) {
            return -1;
        } else if (a.fitness < b.fitness) {
            return 1;
        } else {
            return 0;
        }
    }
    
    //public static int GetRandomWeightedIndex(double[] weights)
    //{
    //    if (weights == null || weights.Length == 0) return -1;

    //    double w;
    //    double t = 0;
    //    int i;
    //    for (i = 0; i < weights.Length; i++)
    //    {
    //        w = weights[i];

    //        if (double.IsPositiveInfinity(w))
    //        {
    //            return i;
    //        }
    //        else if (w >= 0f && !double.IsNaN(w))
    //        {
    //            t += weights[i];
    //        }
    //    }

    //    double r = random.NextDouble();
    //    double s = 0f;

    //    for (i = 0; i < weights.Length; i++)
    //    {
    //        w = weights[i];
    //        if (double.IsNaN(w) || w <= 0f) continue;

    //        s += w / t;
    //        if (s >= r) return i;
    //    }

    //    return -1;
    //}
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
    AGENT,
    NONE
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


