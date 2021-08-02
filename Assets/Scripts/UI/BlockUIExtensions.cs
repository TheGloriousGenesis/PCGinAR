using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    [Serializable]
    public static class BlockUIExtensions
    {
        // checks to see if position is over ui/ gameobject
        public static bool IsPointOverUIObject(this Vector2 pos)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

            PointerEventData eventPostiion = new PointerEventData(EventSystem.current);
            eventPostiion.position = new Vector2(pos.x, pos.y);

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventPostiion, results);

            return results.Count > 0;
        }
        
        public static bool IsTouchingUI(this Vector2 pos) {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
                PointerEventData eventPosition = new PointerEventData(EventSystem.current);
                eventPosition.position = new Vector2(pos.x, pos.y);
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventPosition, results);
                return results.Count > 0;
            } else if (Application.platform == RuntimePlatform.WindowsEditor) {
                return EventSystem.current.IsPointerOverGameObject();
            }
            return true;
        }
    }
}