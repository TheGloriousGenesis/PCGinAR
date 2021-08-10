using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace PathFinding.LinkGenerator
{
    public class NavMeshLinkTbs : NavMeshLink
    {
        [FormerlySerializedAs("animation_FromStart")] public string animationFromStart = "";
        [FormerlySerializedAs("animation_FromEnd")] public string animationFromEnd = "";

        public string GetAnimName(Vector3 charPos)
        {
            return IsTargetNearStart( charPos ) ? animationFromStart : animationFromEnd;
        }

        private bool IsTargetNearStart (Vector3 pos)
        {
            var position = transform.position;
            return Vector3.Distance ( pos, position + startPoint ) < Vector3.Distance ( pos, position + endPoint );
        }

    }
}
