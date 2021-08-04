using System;
using System.Collections.Generic;

namespace GeneticAlgorithms.Entities
{
    [Serializable]
    public class FeatureDimension
    {
        public static Dictionary<string, Func<Chromosome, float>> FeatureFunctionMap = 
            new Dictionary<string, Func<Chromosome, float>>()
            {
                {"NumberOfJumps",CompletionOfLevel},
                {"LengthOfPath", LengthOfPath},
                {"NumberOfTurns", NumberOfTurns}
            };
        
        public string featureName;
        private Func<Chromosome, float> _featureFunction;
        public float target;
        // fix output of this. This is suppose to be a operator
        public int compareFeature;
        public int[] bins;
        
        public FeatureDimension(string featureName)
        {
            this.featureName = featureName;
            _featureFunction = FeatureFunctionMap[featureName];
        }

        public float Calculate(Chromosome chromosome)
        {
            return Math.Abs(_featureFunction(chromosome) - target);
        }

        public int Discretize(float value)
        {
            int index = RetrieveBin(value);
            return index;
        }

        private int RetrieveBin(float val)
        {
            return 0;
        }

        private static float CompletionOfLevel(Chromosome chromosome)
        {
            return 0.0f;
        }
        
        private static float LengthOfPath(Chromosome chromosome)
        {
            return 0.0f;
        }
        
        private static float NumberOfTurns(Chromosome chromosome)
        {
            return 0.0f;
        }
        
    }
}