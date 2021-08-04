using UnityEngine;

namespace Behaviour.Player
{
    public class TestAccelerometer : MonoBehaviour
    {
        // do this to check if phone is flat on surface. This will make Accelerometer Vector3.zero
        public bool isFlat = false;
        private Rigidbody rigid;

        [SerializeField] private float jumpForce = 300f;
        // Start is called before the first frame update
        void Start()
        {
            rigid = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            // compute the tilt away from the normal, which is flat
            Vector3 tilt = Input.acceleration;

            if (isFlat)
            {
                tilt = Quaternion.Euler(90, 0, 0) * tilt;
            }
        
            rigid.AddForce(tilt);

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                rigid.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Force);
            }

            // if (Input.anyKey)
            // {
            //     rigid.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Force);
            // }
        }
    }
}
