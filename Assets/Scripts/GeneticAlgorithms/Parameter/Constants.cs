using Behaviour.Entities;

namespace GeneticAlgorithms.Parameter
{
    public static class Constants
    {
        public const float BLOCK_SIZE = 1.0f;
        public const BlockType PLAYERTYPE = BlockType.AGENT;

        public const int MAX_PLATFORM_DIMENSION = 10;
        public const int CHROMOSONE_LENGTH = 10;
        public const int POPULATION_SIZE = 10;
        public const int ITERATION = 25;

        public const float CROSSOVER_PROBABILITY = 0.6f;
        public const float MUTATION_PROBABILITY = 1 / CHROMOSONE_LENGTH; //1/n where n is length of genome
    
        public const int SEED = 123;
        public const int ELITISM = 5;
    
        // K is used in tournament selection to pick possible parents
        public const int K = 4;
    }
}
