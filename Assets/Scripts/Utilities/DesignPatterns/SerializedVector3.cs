using System;
using UnityEngine;

namespace Utilities.DesignPatterns
{
    [Serializable]
    public struct SerializedVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializedVector3(float paramX, float paramY, float paramZ)
        {
            x = paramX;
            y = paramY;
            z = paramZ;
        }
        
        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}]", x, y, z);
        }
        
        public static implicit operator Vector3(SerializedVector3 rValue)
        {
            return new Vector3(rValue.x, rValue.y, rValue.z);
        }
        
        public static implicit operator SerializedVector3(Vector3 rValue)
        {
            return new SerializedVector3(rValue.x, rValue.y, rValue.z);
        }
    }
}