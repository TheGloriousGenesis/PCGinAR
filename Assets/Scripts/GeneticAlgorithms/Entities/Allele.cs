using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GeneticAlgorithms.Entities
{
    [Serializable]
    public class Allele: Object
    {
        //public Vector3 centerBlock { get; set; }

        [SerializeField]
        public List<Vector3> blockPositions = new List<Vector3>();

        public Allele(List<Vector3> positions)
        {
            blockPositions.AddRange(positions);
        }

        public Allele()
        {

        }

    }
}