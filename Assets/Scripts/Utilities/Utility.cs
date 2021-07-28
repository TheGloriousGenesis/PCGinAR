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
        public static Dictionary<Vector3, BlockType> GamePlacement = new Dictionary<Vector3, BlockType>();
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
            Object.Destroy(obj);

            obj.SetActive(false);
        }

        public static List<T> GetKRandomElements<T>(List<T> list, int k, Random random)
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
            Object.DestroyImmediate(obj);
            //obj.SetActive(false);
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
    
        public static void SaveToFile(Object obj, string path)
        {
            var bf = new BinaryFormatter(); 
            var file = File.Create(path);
            bf.Serialize(file, obj);
            file.Close();
            Debug.Log($"{obj.name} data saved!");
        }

        public static Object LoadFromFile(string path, Object obj)
        {
            var extractFromJson = File.ReadAllText(path);
            return obj;
        }

    }


}