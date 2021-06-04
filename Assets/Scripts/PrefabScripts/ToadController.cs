using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RestClient.Core.Singletons;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class ToadController : Singleton<ToadController>
{
    // Influenced by https://github.com/whl33886/GravityCar/blob/master/Assets/CarTest/CarController.cs
    // Influenced by https://www.youtube.com/watch?v=k2bpIXzwcWA&list=RDCMUCHM37DnT_QGJT5Zyl4EmqcA&start_radio=1&t=1107s

    [SerializeField]
    private Rigidbody _rigidbody;

    [SerializeField]
    private float moveSpeed = 2;

    [SerializeField]
    private float turnSpeed = 3;

    [SerializeField]
    private float jump = 3;

    public bool isGrounded;

    [SerializeField]
    private Animator anim;

    [SerializeField]
    private Transform cameraTransform;

    public int numOfCoins = 0;

    [SerializeField]
    public Text text;
    public enum Direction
    {
        Idle,
        MoveForward,
        MoveBackward,
        TurnLeft,
        TurnRight
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
        var hit = col.gameObject;

        if (hit.tag == "Coin")
        {
            numOfCoins++;
            Destroy(hit);
            text.text = "Coin counter: " + numOfCoins.ToString();
        }
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

        _rigidbody.velocity = targetDirection.normalized * moveSpeed;     //normalized prevents char moving faster than it should with diagonal input

        //    move = move.x * cameraTransform.right + move.z * cameraTransform.forward;
        //    move.y = 0f;
        //    _rigidbody.MovePosition(move * Time.deltaTime * playerSpeed);

        //if (input != Vectowr2.zero)
        //{
        //    float targetAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.z;
        //    Quaternion rotation = Quaternion.Euler(0, targetAngle, 0);
        //    transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime);
        //}
    }

    //private void Awake()
    //{
    //    controller = GetComponent<CharacterController>();
    //    anim = GetComponent<Animator>();
    //    playerInput = GetComponent<PlayerInputController>();
    //    cameraTransform = Camera.main.transform;
    //}

    //void Update()
    //{
    //    groundedPlayer = controller.isGrounded;
    //    if (groundedPlayer && playerVelocity.y < 0)
    //    {
    //        playerVelocity.y = 0f;
    //    }

    //    Vector2 input = PlayerInputController.actions["Move"].ReadValue<Vector2>();
    //    Vector3 move = new Vector3(input.x, 0, input.y);

    //    move = move.x * cameraTransform.right + move.z * cameraTransform.forward;
    //    move.y = 0f;
    //    controller.Move(move * Time.deltaTime * playerSpeed);

    //    // Changes the height position of the player..
    //    if (playerInput.actions["Jump"].triggered && groundedPlayer)
    //    {
    //        // anim.Play("Jump")
    //        playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
    //    }

    //    playerVelocity.y += gravityValue * Time.deltaTime;
    //    controller.Move(playerVelocity * Time.deltaTime);

    //    // Set blending animation when player is moving.
    //    //anim.SetFloat("Blend", input.sqrMagnitude, animationBlendDamp, Time.deltaTime);

    //    // Rotate the player depending on their input and camera direction
    //    if (input != Vector2.zero)
    //    {
    //        float targetAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.z;
    //        Quaternion rotation = Quaternion.Euler(0, targetAngle, 0);
    //        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime);
    //    }
    //}
}

