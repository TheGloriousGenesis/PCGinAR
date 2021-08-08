using System;
using GeneticAlgorithms.Parameter;
using UnityEngine;

namespace Behaviour.Entities
{
    [Serializable]
    public enum BlockType
    {
        BASIC_BLOCK,
        GOAL,
        PLAYER,
        COIN,
        ENEMY_1,
        AGENT,
        NONE,
        FREE_TO_WALK
    }

    [Serializable]
    public class BlockCube
    {
        public BlockType blockType;
        public Vector3 position;

        public BlockCube(BlockType blockType, Vector3 position)
        {
            this.blockType = blockType;
            this.position = position;
        }
        // public float perlinNoiseHighLimit;
    }

    [Serializable]
    public static class BlockPosition
    {
        public static readonly Vector3 UP = new Vector3(0, Constants.BLOCK_SIZE, 0);
        public static readonly Vector3 DOWN = new Vector3(0, -Constants.BLOCK_SIZE, 0);
        public static readonly Vector3 LEFT = new Vector3(-Constants.BLOCK_SIZE, 0, 0);
        public static readonly Vector3 RIGHT = new Vector3(Constants.BLOCK_SIZE, 0, 0);
        public static readonly Vector3 FRONT = new Vector3(0, 0, Constants.BLOCK_SIZE);
        public static readonly Vector3 BACK = new Vector3(0, 0, -Constants.BLOCK_SIZE);
        public static readonly Vector3 NONE = new Vector3(0, 0, 0);
    }

}