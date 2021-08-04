using System;
using System.Collections.Generic;
using Behaviour.Entities;
using UnityEngine;

namespace GeneticAlgorithms.Entities
{
    [Serializable]
    public class Allele
    {
        //public Vector3 centerBlock { get; set; }

        [SerializeField]
        public List<BlockCube> blockPositions = new List<BlockCube>();

        public Allele(List<BlockCube> positions)
        {
            blockPositions.AddRange(positions);
        }

        public Allele()
        {

        }

    }
}