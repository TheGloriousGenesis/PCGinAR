using System.IO;

namespace LoggerFramework
{
    public static class Logger
    {
        private static string corePath = "Assets/results/";
        private static LogBase logger  = new FileLogger(corePath);
        public static void Log(LogTarget target, string message)
        {
            switch (target)
            {
                case LogTarget.BasicGeneticOutput:
                    logger.fileName = "BasicGeneticImplResults.txt";
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

        public string corePath;

        public string fileName;
        public abstract void Log(string message);
            
    }
    
    public class FileLogger : LogBase
    {
        public override void Log(string message)
        {
            lock (lockObj)
            {
                using (StreamWriter streamWriter = new StreamWriter(corePath + fileName, true))
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
