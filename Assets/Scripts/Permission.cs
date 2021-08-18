using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.Android;


public class Permission : MonoBehaviour
{
    void Start()
    {
        AndroidRuntimePermissions.Permission result =
            AndroidRuntimePermissions.RequestPermission("android.permission.ACTIVITY_RECOGNITION");
        if (result == AndroidRuntimePermissions.Permission.Granted)
        {
            ARDebugManager.Instance.LogInfo("We have permission to access the stepcounter");
            AndroidRuntimePermissions.Permission result2 =
                AndroidRuntimePermissions.RequestPermission("android.permission.CAMERA");
            if (result2 == AndroidRuntimePermissions.Permission.Granted)
            {
                ARDebugManager.Instance.LogInfo("We have permission to access the camera");
            }
            else
            {
                ARDebugManager.Instance.LogInfo("We do not have permission to access the camera");
                Debug.Log("Permission state: " + result); // No permission
                // StartCoroutine(CloseAfterTime(10));
            }
        }
        else
        {
            ARDebugManager.Instance.LogInfo("We do not have permission to access the stepcounter");
            Debug.Log("Permission state: " + result); // No permission
            // StartCoroutine(CloseAfterTime(10));
        }
    }

    public IEnumerator CloseAfterTime(float t)
    {
        yield return new WaitForSeconds(t);
        Application.Quit();
    }
}
