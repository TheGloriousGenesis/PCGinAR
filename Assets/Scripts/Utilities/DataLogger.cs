using System.IO;
using UnityEngine;

namespace Utilities
{
    public static class DataLogger
    {
        // todo: this does not save on mobile, sort this out!
        public static readonly string AppPath = Application.persistentDataPath + "/";
        public static readonly string InGameDataPath = Application.persistentDataPath + "/gamesave.json";
        
        private static readonly LogBase logger  = new FileLogger(AppPath);
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
                    logger.fileName = $"MAPElitesImplResults_{variation}.txt";
                    logger.Log(message);
                    break;
                case LogTarget.FI2POP:
                    logger.fileName = $"FI2POPResults_{variation}.txt";
                    logger.Log(message);
                    break;
                case LogTarget.WeightedChunks:
                    logger.fileName = $"WeightedChunks_{variation}.txt";
                    logger.Log(message);
                    break;
                default:
                    Debug.Log("No path has been selected");
                    return;
            }
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
                File.WriteAllText(corePath + fileName, message);
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
        MapElitesOutput,
        FI2POP,
        WeightedChunks
    }
}
