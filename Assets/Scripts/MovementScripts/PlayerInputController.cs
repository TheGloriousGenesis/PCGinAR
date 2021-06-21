//using UnityEngine;

//public class PlayerInputController : MonoBehaviour
//{
//    PlayerControls controls;

//    private void Awake()
//    {
//        controls = new PlayerControls();
//    }

//    void FixedUpdate()
//    {
//        // Movement
//        controls.Player.Move.performed += context => ToadController.Instance.Move(context.ReadValue<Vector2>());
//        controls.Player.Move.canceled += context => ToadController.Instance.Move(Vector2.zero);

//        // Jump
//        controls.Player.Jump.performed += context => ToadController.Instance.Jump(true);
//        controls.Player.Jump.canceled += context => ToadController.Instance.Jump(false);
//    }

//    private void OnEnable()
//    {
//        controls.Enable();
//    }

//    private void OnDisable()
//    {
//        controls.Disable();
//    }
//}
