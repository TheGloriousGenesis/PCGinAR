using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Utilities.ARLogger;

namespace GeneticAlgorithms.Entities
{
    [Serializable]
    public class GameData
    {
        private static GameDataCollection allData;
        public int chromosomeID;
        public int x;
        public int y;
        public int jumps;
        public bool goalReached;
        public double timeCompleted;

        public int numberOfPhysicalMovement;
        public int numberOfDeaths;
        public int numberOfCoinPickups;
    

        public GameData()
        {
        
        }

        public static void SaveGameData(GameData gameData)
        {
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

    public enum GameDataEnum
    {
        X,
        Y,
        JUMP,
        GOAL_REACHED,
        PHYSICAL_MOVEMENT,
        DEATH,
        COIN_PICKUP
    }
}