using System;
using UnityEngine;

namespace Behaviour.Platform.LinkGenerator.Entities
{
    [Serializable]
    public class Edge
    {
        public Vector3 start;
        public Vector3 end;

        public Vector3 startUp;
        public Vector3 endUp;

        public float length;
        public Quaternion facingNormal;
        public bool facingNormalCalculated = false;


        public Edge(Vector3 startPoint, Vector3 endPoint)
        {
            start = startPoint;
            end = endPoint;
        }
    }
}