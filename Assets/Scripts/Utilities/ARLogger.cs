using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GeneticAlgorithms.Entities;
using UnityEngine;

namespace Utilities
{
    public static class Logger
    {
        // todo: this does not save on mobile, sort this out!
        private static readonly string corePath = Path.Combine(Application.persistentDataPath,"Assets/results/RawData/");
        private static readonly LogBase logger  = new FileLogger(corePath);
        //put method in here to set up file 
        public static void Log(LogTarget target, string variation, string message)
        {
            switch (target)
            {
                case LogTarget.BasicGeneticOutput:
                    logger.fileName = $"BasicGeneticImplResults_{variation}.txt";
                    logger.Log(message);
                    break;
                case LogTarget.MapElitesOutput:
                    logger.fileName = "MAPElitesImplResults.txt";
                    logger.Log(message);
                    break;
                default:
                    return;
            }
    
        }

        public static void SaveGame(GameData gameData)
        {
            string json = JsonUtility.ToJson(gameData);
            
            Debug.Log("Saving as JSON: " + json);
            FileInfo file = new System.IO.FileInfo(corePath + "GameData/gamesave.json");
            file.Directory?.Create();
            Debug.Log(file.FullName);
            File.WriteAllText(file.FullName, json);
            
            // BinaryFormatter bf = new BinaryFormatter();
            // FileStream file = File.Create(corePath + "GameData/gamesave.save");
            // bf.Serialize(file, gameData);
            // file.Close();
            // Debug.Log("Game Saved");
        }
        
        public static void LoadGame(GameData gameData)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(corePath + "GameData/gamesave.save");
            bf.Serialize(file, gameData);
            file.Close();
            Debug.Log("Game Saved");
        }
    }
    
    public abstract class LogBase
    {
        protected readonly object lockObj = new object();

        protected string corePath;

        public string fileName;
        public abstract void Log(string message);
            
    }
    
    public class FileLogger : LogBase
    {
        public override void Log(string message)
        {
            lock (lockObj)
            {
                using (var streamWriter = new StreamWriter(corePath + fileName, true))
                {
                    streamWriter.WriteLine(message);
                    streamWriter.Close();
                }
            }
        }
    
        public FileLogger(string corePath)
        {
            this.corePath = corePath;
            
        }
    }
        
    public enum LogTarget
    {
        BasicGeneticOutput,
        MapElitesOutput
    }
}
