using System.Collections.Generic;
using System.Linq;
using Behaviour.Entities;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Utilities;

namespace Behaviour.Platform.LinkGenerator
{
    public class GenerateNavLinks : MonoBehaviour
    {
        public float linkWidth;
        public bool bidirectionalLinks;
        
        public NavMeshLink linkPrefab;

        Vector3 closestPointFromAToB;
        Vector3 closestPointFromBToA;
        public float linkCompenstationAmount;
        private List<BoxCollider> floors = new List<BoxCollider>();
        public bool debugLines;
        [FormerlySerializedAs("wallConnectThreshold")] public float brickConnectThreshold;
        // BoxCollider[] allBoxes;
        
        public void DoGenerateLinks()
        {
            List<Vector3> walkable = Utility.GetGameMap()[BlockType.FREE_TO_WALK];
            foreach (Transform  i in transform)
            {
                if (walkable.Contains(i.position)) floors.Add(i.GetComponent<BoxCollider>());
            }
            // floors = GetComponentsInChildren<BoxCollider>().ToList();
            ConnectThemAll();
        }

        public void ClearLinks()
        {
            var navMeshLinkList = GetComponentsInChildren<NavMeshLink>().ToList();
            for (int i=0; i < navMeshLinkList.Count; i++)
            {
                var obj = navMeshLinkList[i].gameObject;
                if (obj != null) Destroy(obj);
            }
        }
        public void ConnectThemAll()
        {
            IfDistanceOkThenConnect(floors, floors);
        }

        public void IfDistanceOkThenConnect(List<BoxCollider> aList, List<BoxCollider> bList)
        {
            for (int index = 0; index < aList.Count; index++)
            {
                var i = aList[index];

                for (int index1 = 0; index1 < bList.Count; index1++)
                {
                    var ii = bList[index1];

                    if (IsObjectCloseEnough(i, ii))
                    {
                        ConnectTheLinks(i, ii);
                    }

                }
            }

        }

        public bool IsObjectCloseEnough(BoxCollider a, BoxCollider b)
        {

            if (a == b)
            {
                return false;
            }

            var boxCenter = a.center;
            var aCenter = a.transform.TransformPoint(boxCenter);

            var closestFromAToB = a.ClosestPoint(b.ClosestPoint(aCenter));
            var closestFromBToA = b.ClosestPoint(closestFromAToB);
            var distance = Vector3.Distance(closestFromAToB, closestFromBToA);

            if (distance <= brickConnectThreshold && distance >= 1.4f)
            {
                return true;
            } else
            {
                return false;
            }

        }

        public void ConnectTheLinks(BoxCollider a, BoxCollider b)
        {
            GetClosestPointsToEachOther(a, b);
            var link = CreateLinkOnCollider(a);
            SetNavMeshLinkData(link, a);
            // AdjustLinks(link, a, b);
        }

        public void GetClosestPointsToEachOther(BoxCollider a, BoxCollider b)
        {
            var aCenter = GetBoxCenterPosition(a, a.transform);
            closestPointFromAToB = a.ClosestPoint(b.ClosestPoint(aCenter));
            closestPointFromBToA = b.ClosestPoint(closestPointFromAToB);
        }

        public NavMeshLink CreateLinkOnCollider(BoxCollider coll)
        {
            return coll.gameObject.AddComponent<NavMeshLink>();
        }

        public void SetNavMeshLinkData(NavMeshLink link, BoxCollider a)
        {
            link.startPoint = a.transform.InverseTransformPoint(closestPointFromAToB);
            link.endPoint = a.transform.InverseTransformPoint(closestPointFromBToA);
            link.bidirectional = bidirectionalLinks;
            link.width = linkWidth;
        }

        public void AdjustLinks(NavMeshLink link, BoxCollider a, BoxCollider b)
        {
            var aCenter = GetBoxCenterPosition(a, a.transform);

            var directionFromACenterToLinkStart = -(closestPointFromAToB - aCenter).normalized;
            if (debugLines == true)
            {
                Debug.DrawRay(closestPointFromAToB, directionFromACenterToLinkStart, Color.green, 99);
            }

            Ray aRay = new Ray(closestPointFromAToB, directionFromACenterToLinkStart);
            var aPos = aRay.GetPoint(linkCompenstationAmount);

            var bCenter = GetBoxCenterPosition(b, b.transform);

            var directionFromBTransformToLinkEnd = -(closestPointFromBToA - bCenter).normalized;
            if (debugLines == true)
            {
                Debug.DrawRay(closestPointFromBToA, directionFromBTransformToLinkEnd, Color.red, 99);
            }

            Ray bRay = new Ray(closestPointFromBToA, directionFromBTransformToLinkEnd);
            var bPos = bRay.GetPoint(linkCompenstationAmount);


            link.startPoint = a.transform.InverseTransformPoint(aPos);
            link.endPoint = a.transform.InverseTransformPoint(bPos);
        }

        public Vector3 GetBoxCenterPosition(BoxCollider coll, Transform trans)
        {
            var box = coll.center;
            return trans.transform.TransformPoint(box);
        }

    }
}