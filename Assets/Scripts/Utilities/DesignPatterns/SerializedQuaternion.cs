using System;
using UnityEngine;

namespace Utilities.DesignPatterns
{
    [Serializable]
    public class SerializedQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public SerializedQuaternion(float rX, float rY, float rZ, float rW)
        {
            x = rX;
            y = rY;
            z = rZ;
            w = rW;
        }

        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
        }
        
        public static implicit operator Quaternion(SerializedQuaternion rValue)
        {
            return new Quaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }
        
        public static implicit operator SerializedQuaternion(Quaternion rValue)
        {
            return new SerializedQuaternion(rValue.x, rValue.y, rValue.z, rValue.w);
        }
    }
}
