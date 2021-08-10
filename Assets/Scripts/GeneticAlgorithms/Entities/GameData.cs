using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UI;
using UnityEngine;
using static Utilities.ARLogger;

namespace GeneticAlgorithms.Entities
{
    [Serializable]
    public class GameData
    {
        private static GameDataCollection allData;
        
        public int chromosomeID;
        
        #region Game metrics
        public float numberOfInGameMovements;
        public float numberOfJumps;
        public bool goalReached;
        #endregion
        
        #region AR metrics
        public double timeCompleted;
        public float numberOfPhysicalMovement;
        #endregion
        
        public GameData()
        {
        }

        public static void SaveGameData(GameData gameData)
        {
            ARDebugManager.Instance.LogInfo("Saving GameData...");
            string json;
            if (allData == null)
            {
                allData = new GameDataCollection();
            }
            else
            {
                var inputString = File.ReadAllText(InGameDataPath);
                allData = JsonUtility.FromJson<GameDataCollection>(inputString);
            }
            List<GameData> dataAsList = allData.data.ToList();
            dataAsList.Add(gameData);
            allData.data = dataAsList;
            json = JsonUtility.ToJson(allData);
            
            FileInfo file = new FileInfo(InGameDataPath);
            file.Directory?.Create();
            File.WriteAllText(file.FullName, json); 
    }
        
        public static GameDataCollection LoadGameData()
        {
            var inputString = File.ReadAllText(InGameDataPath);
            allData = JsonUtility.FromJson<GameDataCollection>(inputString);
            return allData;
        }
    }

    [Serializable]
    public class GameDataCollection
    {
        [SerializeField]
        public List<GameData> data = new List<GameData>();
    }

    [Serializable]
    public enum Challenge
    {
        EASY,
        HARD,
        NEUTRAL,
        IMPOSSIBLE
    }
}