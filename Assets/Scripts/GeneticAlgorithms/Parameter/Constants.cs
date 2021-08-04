using System;
using Behaviour.Entities;

namespace GeneticAlgorithms.Parameter
{
    public static class Constants
    {
        public const float BLOCK_SIZE = 1.0f;
        public const BlockType PLAYERTYPE = BlockType.PLAYER;

        public const int MAX_PLATFORM_DIMENSION_X = 10;
        public const int MAX_PLATFORM_DIMENSION_Y = 5;
        public const int MAX_PLATFORM_DIMENSION_Z = 10;
        
        private static int extra = 2 * (Constants.MAX_PLATFORM_DIMENSION_X * Constants.MAX_PLATFORM_DIMENSION_Y +
                                      Constants.MAX_PLATFORM_DIMENSION_X * Constants.MAX_PLATFORM_DIMENSION_Z +
                                      Constants.MAX_PLATFORM_DIMENSION_Y * Constants.MAX_PLATFORM_DIMENSION_Z + 
                                      2 * Constants.MAX_PLATFORM_DIMENSION_X +
                                      2 * Constants.MAX_PLATFORM_DIMENSION_Y +
                                      2 * Constants.MAX_PLATFORM_DIMENSION_Z +
                                      4);
            
        public static int TOTAL_MAXIMUM_AREA = (Constants.MAX_PLATFORM_DIMENSION_X + 2) *
                                    (Constants.MAX_PLATFORM_DIMENSION_Z + 2) *
                                    (Constants.MAX_PLATFORM_DIMENSION_Y + 2);
        
        // at least half the area should be covered in chromosomes
        public static readonly int CHROMOSONE_LENGTH = (int) Math.Ceiling((TOTAL_MAXIMUM_AREA - extra) *
                                                                          (1.0/4.0) * (1.0/3.0));

        public const int POPULATION_SIZE = 15;
        
        public const int ITERATION = 10;

        public const float CROSSOVER_PROBABILITY = 0.6f;
        public static readonly float MUTATION_PROBABILITY = 1f / CHROMOSONE_LENGTH;
        // public static readonly int NUMBER_OF_MUTATION = (int) Math.Ceiling(CHROMOSONE_LENGTH * 0.25);

        public const int SEED = 123;
        public const int ELITISM = 1;
    
        // K is used in tournament selection to pick possible parents
        public const int K = 2;
    }
}
