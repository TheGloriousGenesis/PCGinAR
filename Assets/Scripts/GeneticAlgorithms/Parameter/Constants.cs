using System;
using Behaviour.Entities;

namespace GeneticAlgorithms.Parameter
{
    public static class Constants
    {
        public const float BLOCK_SIZE = 1.0f;
        public const BlockType PLAYERTYPE = BlockType.PLAYER;

        public const int MAX_PLATFORM_DIMENSION_X = 5;
        public const int MAX_PLATFORM_DIMENSION_Y = 2;
        public const int MAX_PLATFORM_DIMENSION_Z = 5;
        
        public static int TOTAL_VOLUMNE = ((Constants.MAX_PLATFORM_DIMENSION_X + 2) *
                                    (Constants.MAX_PLATFORM_DIMENSION_Z + 2) *
                                    (Constants.MAX_PLATFORM_DIMENSION_Y + 2)) - 
                                               (2 * (MAX_PLATFORM_DIMENSION_X * MAX_PLATFORM_DIMENSION_Y +
                                                     MAX_PLATFORM_DIMENSION_X * MAX_PLATFORM_DIMENSION_Z +
                                                     MAX_PLATFORM_DIMENSION_Y * MAX_PLATFORM_DIMENSION_Z + 
                                                     2 * MAX_PLATFORM_DIMENSION_X +
                                                     2 * MAX_PLATFORM_DIMENSION_Y +
                                                     2 * MAX_PLATFORM_DIMENSION_Z +
                                                     4));
        
        // at least half the area should be covered in chromosomes
        public static readonly int CHROMOSONE_LENGTH = (int) Math.Ceiling(TOTAL_VOLUMNE *
                                                                          (1.0/4.0) * (1.0/3.0));

        public const int POPULATION_SIZE = 6;
        
        public const int ITERATION = 10;

        public const float CROSSOVER_PROBABILITY = 0.6f;
        public static readonly float MUTATION_PROBABILITY = 1f / CHROMOSONE_LENGTH;
        // public static readonly int NUMBER_OF_MUTATION = (int) Math.Ceiling(CHROMOSONE_LENGTH * 0.25);

        public const int SEED = 123;
        public const int ELITISM = 1;
    
        // K is used in tournament selection to pick possible parents
        public const int K = 2;
        
        public const int NUMBER_OF_CHUNKS = 16;
    }
}
