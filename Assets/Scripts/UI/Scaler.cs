using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace UI
{
    // influenced by https://forum.unity.com/threads/scaling-with-arfoundation.543953/
    [RequireComponent(typeof(ARSessionOrigin))]
    public class Scaler : MonoBehaviour {
 
        ARSessionOrigin m_SessionOrigin;
        public GameObject referenceToScale;
 
        /* Next values must be the same min and max values of
     * the Slider to change the scale */
        private float m_maxScaleValue = 10f;
        private float m_minScaleValue = 1.0f;
        private float m_defaultScaleValue = 5.0f;

        // public void OnEnable()
        // {
        //     EventManager.OnGameStart += ResetReference;
        // }

        // private void ResetReference()
        // {
        //     referenceToScale.transform.position = new Vector3();
        //     referenceToScale.transform.rotation = new Quaternion();
        // }

        // public void OnDisable()
        // {
        //     EventManager.OnGameStart -= ResetReference;
        // }

        void Awake()
        {
            m_SessionOrigin = GetComponent<ARSessionOrigin>();
        }
 
        // Method called by a Slider
        public void OnValueChange(float value)
        {
            // session ord
            Transform t = gameObject.transform;
            
            m_SessionOrigin.MakeContentAppearAt(
                referenceToScale.transform,
                referenceToScale.transform.position,
                referenceToScale.transform.rotation);
 
            float scaleValue = Mathf.Clamp(value, m_minScaleValue, m_maxScaleValue);
            t.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
        }
 
        private void Start()
        {
            OnValueChange(m_defaultScaleValue);
        }
    }
}