using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RestClient.Core.Singletons;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class ToadController : Singleton<ToadController>
{
    // Influenced by https://github.com/whl33886/GravityCar/blob/master/Assets/CarTest/CarController.cs
    // Influenced by https://www.youtube.com/watch?v=k2bpIXzwcWA&list=RDCMUCHM37DnT_QGJT5Zyl4EmqcA&start_radio=1&t=1107s

    private Rigidbody _rigidbody;

    [SerializeField]
    private float moveSpeed = 1f;

    [SerializeField]
    private float turnSpeed = 3;

    [SerializeField]
    private float jump = 3f;

    public bool isGrounded;

    private Animator anim;

    [SerializeField]
    private Transform cameraTransform;

    private Text text;

    public bool falling = false;

    public float fallingThreshold = -9f;

    public int numOfCoins = 0;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        text = GameObject.Find("PlayerStatCanvas").GetComponent<Text>();
    }

    public void Jump(bool isJumping)
    {
        if (isGrounded)
        {
            _rigidbody.AddForce(jump * new Vector3(0, 2, 0), ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionStay()
    {
        isGrounded = true;
    }

    void OnCollisionEnter(Collision col) { 
        isGrounded = true;
    }

    void OnCollisionExit(Collision other) { isGrounded = false; }

    public void Move(Vector2 input)
    {
        Vector3 dir = Vector3.zero;

        dir.x = input.x;
        dir.z = input.y;

        Vector3 camDirection = Camera.main.transform.rotation * dir; //This takes all 3 axes (good for something flying in 3d space)    
        Vector3 targetDirection = new Vector3(camDirection.x, 0, camDirection.z); //This line removes the "space ship" 3D flying effect. We take the cam direction but remove the y axis value

        if (dir != Vector3.zero)
        { //turn the character to face the direction of travel when there is input
            transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(targetDirection),
            Time.deltaTime * turnSpeed
            );
        }

        //normalized prevents char moving faster than it should with diagonal input
        _rigidbody.velocity = targetDirection.normalized * moveSpeed;     
    }


    public void Update()
    {
        if (this.gameObject.activeSelf)
        {
            CheckIfPlayerHasFallen();
        }
        text.text = "Coin counter: " + numOfCoins.ToString();
    }

    public void CheckIfPlayerHasFallen()
    {
        if (_rigidbody.velocity.y  < fallingThreshold)
        {
            falling = true;
        } else
        {
            falling = false;
        }

        if (falling)
        {
            numOfCoins -= 100;
            text.text = "Coin counter: " + numOfCoins.ToString();
            Utility.SafeDestory(this.gameObject);
        }
    }
}

