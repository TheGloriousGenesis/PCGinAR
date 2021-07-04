﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public const float BLOCK_SIZE = 1.0f;
    public const float P_BLOCK_BREAK = 0.5f;

    public const int MAX_PLATFORM_DIMENSION = 10;
    public const int CHROMOSONE_LENGTH = 10;
    public const float CROSSOVER_PROBABILITY = 0.2f;
    public const float MUTATION_PROBABILITY = 0.2f; //1/n where n is length of genome
    public const int ITERATION = 1;
    public const int POPULATION_SIZE = 30;
    public const BlockType playerType = BlockType.AGENT;

    public const int SEED = 123;
    public const int ELITISM = 5;
}
