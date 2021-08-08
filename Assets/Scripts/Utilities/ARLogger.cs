using System.IO;
using UnityEngine;

namespace Utilities
{
    public static class ARLogger
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
                    logger.fileName = "MAPElitesImplResults.txt";
                    logger.Log(message);
                    break;
                default:
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
                using (var streamWriter = new StreamWriter(corePath + fileName, true))
                {
                    streamWriter.WriteLine(message);
                    streamWriter.Close();
                }
            }
        }
    
        public FileLogger(string corePath)
        {
            Debug.Log($"Corepath: {corePath}");
            this.corePath = corePath;
            
        }
    }
        
    public enum LogTarget
    {
        BasicGeneticOutput,
        MapElitesOutput
    }
}
