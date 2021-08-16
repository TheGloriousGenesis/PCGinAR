using System;
using System.Collections.Generic;
using Behaviour.Entities;
using UnityEngine;
using Utilities.DesignPatterns;

namespace GeneticAlgorithms.Entities
{
    [Serializable]
    public class Allele
    {
        // private static float[] _weightedChunks; 

        //     [SerializeField]
        // public List<BlockCube> blockPositions = new List<BlockCube>();

        public SerializedVector3 position;

        public int chunkID;
        
        public Allele(Vector3 position, int chunkID)
        {
            this.position = position;
            this.chunkID = chunkID;
        }

        public Allele()
        {

        }

        // public float[] GetWeightedChunks()
        // {
        //     if (_weightedChunks == null)
        //     {
        //         _weightedChunks = new float[16];
        //         for(int i=0; i < _weightedChunks.Length; i++)
        //         {
        //             _weightedChunks[i] = 1f / 16;
        //         }
        //     }
        //
        //     return _weightedChunks;
        // }
        //
        // public void SetWeightedChunks(float[] values)
        // {
        //     _weightedChunks = values;
        // }

    }
}