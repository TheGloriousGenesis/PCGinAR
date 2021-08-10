// using TMPro;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using Gyroscope = UnityEngine.InputSystem.Gyroscope;
//
// namespace UI
//  {
//      [DefaultExecutionOrder(-1)]
//      public class Sensors : MonoBehaviour {
//          public TextMeshProUGUI textaccel;
//          public TextMeshProUGUI textgyro;
//          public TextMeshProUGUI textlinearaccel;
//          public TextMeshProUGUI textstepcount;
//      
//          Accelerometer accelerometer;
//          Gyroscope gyroscope;
//          LinearAccelerationSensor linearaccel;
//          StepCounter step;
//
//          private int resetStep;
//          private bool firstEncounter = true;
//          private int currentRun;
//
//
//          private void Awake()
//          {
//              AndroidRuntimePermissions.Permission result =
//                  AndroidRuntimePermissions.RequestPermission("android.permission.ACTIVITY_RECOGNITION");
//              if (result == AndroidRuntimePermissions.Permission.Granted)
//              {
//                  Debug.Log("We have permission to access the stepcounter");
//              }
//              else
//              {
//                  Debug.Log("Permission state: " + result); // No permission
//                  Application.Quit();
//              }
//
//          }
//
//          void Start()
//          {
//              accelerometer = Accelerometer.current;
//              gyroscope = Gyroscope.current;
//              linearaccel = LinearAccelerationSensor.current;
//              step = StepCounter.current;
//              
//              if (accelerometer != null)
//              {
//                  InputSystem.EnableDevice(accelerometer);
//                  textaccel.text = "accelerometer enabled";
//              }
//              else
//              {
//                  textaccel.text = "accelerometer null";
//      
//              }
//                    
//              if (gyroscope != null)
//              {
//                  InputSystem.EnableDevice(gyroscope);
//                  textgyro.text = "gyroscope enabled";
//              }
//              else
//              {
//                  textgyro.text = "gyroscope null";
//      
//              }
//
//              if (linearaccel != null)
//              {
//                  InputSystem.EnableDevice(linearaccel);
//                  textlinearaccel.text = "linearaccel enabled";
//              }
//              else
//              {
//                  textlinearaccel.text = "linearaccel null";
//      
//              }
//
//              if (step != null)
//              {
//                  InputSystem.EnableDevice(step);
//                  textstepcount.text = "step enabled";
//              }
//              else
//              {
//                  textstepcount.text = "step null";
//              }
//          }
//
//          private void OnApplicationQuit()
//          { 
//              if (accelerometer != null)
//              {
//                  InputSystem.DisableDevice(accelerometer);
//              }
//              if (gyroscope != null)
//              {
//                  InputSystem.DisableDevice(gyroscope);
//                  textgyro.text = "gyroscope enabled";
//              }
//
//              if (linearaccel != null)
//              {
//                  InputSystem.DisableDevice(linearaccel);
//                  textlinearaccel.text = "linearaccel enabled";
//              }
//              if (step != null)
//              {
//                  InputSystem.DisableDevice(step);
//              }
//          }
//
//          void Update()
//          {
//               if (accelerometer != null)
//               {
//                   textaccel.text = $"accelerometer: {accelerometer.acceleration.ReadValue()}";
//               }
//
//               if (gyroscope != null)
//               {
//                   textgyro.text = $"gyroscope: {gyroscope.angularVelocity.ReadValue()}";
//                   // threshold left to right: 0.3
//
//                   if (gyroscope.angularVelocity.ReadValue().x >= 0.3)
//                       Debug.Log($"G X axis = {gyroscope.angularVelocity.ReadValue().x}");
//                   
//                   if (gyroscope.angularVelocity.ReadValue().y >= 0.3)
//                       // threshold up to down: 0.3
//                       Debug.Log($"G Y axis = {gyroscope.angularVelocity.ReadValue().y}");
//                   
//                   if (gyroscope.angularVelocity.ReadValue().z >= 0.3)
//                       // threshold up to down: 0.3
//                       Debug.Log($"G Z axis = {gyroscope.angularVelocity.ReadValue().z}");
//               }
//
//
//               if (linearaccel != null)
//               {
//                   textlinearaccel.text = $"linearaccel: {linearaccel.acceleration.ReadValue()}";
//                   Debug.Log($"L X axis = {linearaccel.acceleration.ReadValue().x}");
//                   Debug.Log($"L Y axis = {linearaccel.acceleration.ReadValue().y}");
//                   Debug.Log($"L Z axis = {linearaccel.acceleration.ReadValue().z}");
//               }
//
//               if (step != null)
//               {
//                   if (firstEncounter)
//                   {
//                       resetStep = step.stepCounter.ReadValue();
//                       resetStep = step.stepCounter.ReadValue();
//                       firstEncounter = false;
//                       Debug.Log($"ResetStep count: {resetStep}");
//                   }
//                   
//                   textstepcount.text = $"step: {step.stepCounter.ReadValue() - resetStep }";
//
//                   Debug.Log($"AC X axis = {accelerometer.acceleration.ReadValue().x}");
//                   Debug.Log($"AC Y axis = {accelerometer.acceleration.ReadValue().y}");
//                   Debug.Log($"AC Z axis = {accelerometer.acceleration.ReadValue().z}");
//                   
//                   // detect(Vector3.SqrMagnitude(accelerometer.acceleration.ReadValue()), 
//                   //     1.3d);
//                   // Debug.Log($"Make shift step count: {getStepCount()}");
//               }
//               
//          }
//      }
//  }
