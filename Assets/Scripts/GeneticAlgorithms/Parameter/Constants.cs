using System;
using Behaviour.Entities;

namespace GeneticAlgorithms.Parameter
{
    public static class Constants
    {
        public const float BLOCK_SIZE = 0.5f;
        public const BlockType PLAYERTYPE = BlockType.PLAYER;

        public const int MAX_PLATFORM_DIMENSION_X = 10;
        public const int MAX_PLATFORM_DIMENSION_Y = 2;
        public const int MAX_PLATFORM_DIMENSION_Z = 3;
        
        public static int TOTAL_MOCK_VOLUME = TOTAL_VOLUME - 
                                               (2 * (MAX_PLATFORM_DIMENSION_X * MAX_PLATFORM_DIMENSION_Y +
                                                     MAX_PLATFORM_DIMENSION_X * MAX_PLATFORM_DIMENSION_Z +
                                                     MAX_PLATFORM_DIMENSION_Y * MAX_PLATFORM_DIMENSION_Z + 
                                                     2 * MAX_PLATFORM_DIMENSION_X +
                                                     2 * MAX_PLATFORM_DIMENSION_Y +
                                                     2 * MAX_PLATFORM_DIMENSION_Z +
                                                     4));

        public static int TOTAL_VOLUME = (MAX_PLATFORM_DIMENSION_X + 2) *
                                           (MAX_PLATFORM_DIMENSION_Z + 2) *
                                           (MAX_PLATFORM_DIMENSION_Y + 2);
        // at least half the area should be covered in chromosomes
        public static int CHROMOSOME_LENGTH = (int) Math.Ceiling(TOTAL_MOCK_VOLUME *
                                                                 (1.0/3.0));

        public const int POPULATION_SIZE = 10;
        
        public const int NUMBER_OF_GENERATIONS = 10;

        public const float CROSSOVER_PROBABILITY = 0.6f;
        public static readonly float MUTATION_PROBABILITY = 1f / CHROMOSOME_LENGTH;

        public static int MAX_NUMBER_OF_MUTATIONS = 4;
        // public static readonly int NUMBER_OF_MUTATION = (int) Math.Ceiling(CHROMOSONE_LENGTH * 0.25);

        public const int SEED = 123;
        public const int ELITISM = 1;
    
        // K is used in tournament selection to pick possible parents
        public const int K = 2;
        
        public const int NUMBER_OF_CHUNKS = 16;
    }
}
