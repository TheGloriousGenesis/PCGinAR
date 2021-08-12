using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using Behaviour.Entities;
using GeneticAlgorithms.Entities;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Utilities
{
    [Serializable]
    public static class Utility
    {
        private static Dictionary<BlockType, List<Vector3>> GameMap = new Dictionary<BlockType, List<Vector3>>();
        
        public static List<Vector3> EdgesOfCurrentGame { get; set; }

        #region GameMap Methods
        public static Dictionary<BlockType, List<Vector3>> GetGameMap()
        {
            if (GameMap != null)
                return GameMap;
            GameMap = new Dictionary<BlockType, List<Vector3>>();
            foreach (BlockType cube in Enum.GetValues(typeof(BlockType)))
            {
                GameMap[cube] = new List<Vector3>();
            }

            return GameMap;
        }

        public static void ResetGameMap()
        {
            GameMap = new Dictionary<BlockType, List<Vector3>>();
        }

        public static void SetGameMap(Dictionary<BlockType, List<Vector3>> dict)
        {
            GameMap = dict;
        }
        
        public static List<Vector3> GetKeyFromValue(Dictionary<Vector3, BlockType> dic, BlockType value)
        {
            var allPositions = new List<Vector3>();

            foreach(var i in dic)
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
            var replacableKeys = new List<Vector3>();

            foreach(var i in dic)
            {
                if (i.Value == replace)
                {
                    replacableKeys.Add(i.Key);
                }
            }
        
            foreach(var i in replacableKeys)
            {
                dic[i] = value;
            }
        }

        #endregion
        
        #region Destroy Methods
        // Inspired from https://forum.unity.com/threads/so-why-is-destroyimmediate-not-recommended.526939/
        public static void SafeDestroyInPlayMode(GameObject obj)
        {
            obj.transform.parent = null;
            obj.name = "$disposed";
            Object.Destroy(obj);
            obj.SetActive(false);
        }

        public static void SafeDestroyInEditMode(GameObject obj)
        {
            obj.transform.parent = null;
            obj.name = "$disposed";
            Object.DestroyImmediate(obj);
        }

        #endregion
        
        #region Misc
        
        // public static IEnumerable<float> Range(float min, float max, float step)
        // {
        //     float i;
        //     for (i=min; i<=max; i+=step)
        //         yield return i;
        //
        //     if (Math.Abs(i - (max+step)) > 0.001) // added only because you want max to be returned as last item
        //         yield return max; 
        // }
        
        public static float[] Range(float min, float max, float step)
        {
            float[] tmp = new float[(int) (max / step) + 1];
            int count = 0;
            for (float i = min; i <= max; i += step)
            {
                tmp[count] = i;
                count++;
            }
            return tmp;
        }
        
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        
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

        public static List<T> GetKRandomElements<T>(IEnumerable<T> list, int k, Random random)
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

        public static int CompareChromosome(Chromosome a, Chromosome b)
        {
            if (a.Fitness > b.Fitness) {
                return -1;
            }

            return a.Fitness < b.Fitness ? 1 : 0;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetTotalDistance(this NavMeshPath targetNavMeshPath)
        {
            float calculatedLength = 0;

            // The iterator looks ahead one position
            var count = targetNavMeshPath.corners.Length - 1;

            // Look at points in the NavMeshPath
            for (var i = 0; i < count; i++)
            {
                calculatedLength += Vector3.Distance(targetNavMeshPath.corners[i], targetNavMeshPath.corners[i + 1]);
            }

            return calculatedLength;
        }
        
        #endregion

        #region Data capturing
        // public static void SaveToFile(Object obj, string path)
        // {
        //     var bf = new BinaryFormatter(); 
        //     var file = File.Create(path);
        //     bf.Serialize(file, obj);
        //     file.Close();
        //     Debug.Log($"{obj.name} data saved!");
        // }
        //
        // public static Object LoadFromFile(string path, Object obj)
        // {
        //     var extractFromJson = File.ReadAllText(path);
        //     return obj;
        // }

        #endregion
    }

    [Serializable]
    public sealed class FloatEqualityComparer : EqualityComparer<float>
    {
        public override bool Equals(float x, float y) => GetEquatable(x) == GetEquatable(y);
    
        public override int GetHashCode(float f) => GetEquatable(f).GetHashCode();

        private static float GetEquatable(float f) => (float) Math.Round(f, 3);
    }

}