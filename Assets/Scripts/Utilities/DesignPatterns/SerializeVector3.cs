using System;

namespace Utilities.DesignPatterns
{
    [Serializable]
    public struct SerializeVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializeVector3(float paramX, float paramY, float paramZ)
        {
            x = paramX;
            y = paramY;
            z = paramZ;
        }
    }
}