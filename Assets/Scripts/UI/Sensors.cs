using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Gyroscope = UnityEngine.InputSystem.Gyroscope;

namespace UI
 {
     public class Sensors : MonoBehaviour {
         public TextMeshProUGUI textaccel;
         public TextMeshProUGUI textgyro;
         public TextMeshProUGUI textgravity;
         public TextMeshProUGUI textattitude;
         public TextMeshProUGUI textlinearaccel;
         public TextMeshProUGUI textmagfield;
         public TextMeshProUGUI textlight;
         public TextMeshProUGUI textpressure;
         public TextMeshProUGUI texthumidity;
         public TextMeshProUGUI texttemp;
         public TextMeshProUGUI textstepcount;
     
         Accelerometer accelerometer;
         Gyroscope gyroscope;
         GravitySensor gravity;
         AttitudeSensor attitude;
         LinearAccelerationSensor linearaccel;
         MagneticFieldSensor magfield;
         LightSensor lightsens;
         PressureSensor pressure;
         HumiditySensor humidity;
         AmbientTemperatureSensor ambient;
         StepCounter step;
     
         void Start()
         {
             accelerometer = Accelerometer.current;
             gyroscope = Gyroscope.current;
             gravity = GravitySensor.current;
             attitude = AttitudeSensor.current;
             linearaccel = LinearAccelerationSensor.current;
             magfield = MagneticFieldSensor.current;
             lightsens = LightSensor.current;
             pressure = PressureSensor.current;
             humidity = HumiditySensor.current;
             ambient = AmbientTemperatureSensor.current;
             step = StepCounter.current;
             
             if (accelerometer != null)
             {
                 InputSystem.EnableDevice(accelerometer);
                 textaccel.text = "accelerometer enabled";
             }
             else
             {
                 textaccel.text = "accelerometer null";
     
             }
                   
             if (gyroscope != null)
             {
                 InputSystem.EnableDevice(gyroscope);
                 textgyro.text = "gyroscope enabled";
             }
             else
             {
                 textgyro.text = "gyroscope null";
     
             }
     
             if (gravity != null)
             {
                 InputSystem.EnableDevice(gravity);
                 textgravity.text = "gravity enabled";
             }
             else
             {
                 textgravity.text = "gravity null";
     
             }
     
             if (attitude != null)
             {
                 InputSystem.EnableDevice(attitude);
                 textattitude.text = "attitude enabled";
             }
             else
             {
                 textattitude.text = "attitude null";
             }
     
             if (linearaccel != null)
             {
                 InputSystem.EnableDevice(linearaccel);
                 textlinearaccel.text = "linearaccel enabled";
             }
             else
             {
                 textlinearaccel.text = "linearaccel null";
     
             }
     
             if (magfield != null)
             {
                 InputSystem.EnableDevice(magfield);
                 textmagfield.text = "magfield enabled";
             }
             else
             {
                 textmagfield.text = "magfield null";
     
             }
     
             if (lightsens != null)
             {
                 InputSystem.EnableDevice(lightsens);
                 textlight.text = "lightsens enabled";
             }
             else
             {
                 textlight.text = "lightsens null";
             }
     
             if (pressure != null)
             {
                 InputSystem.EnableDevice(pressure);
                 textpressure.text = "pressure enabled";
             }
             else
             {
                 textpressure.text = "pressure null";        
             }
     
             if (humidity != null)
             {
                 InputSystem.EnableDevice(humidity);
                 texthumidity.text = "humidity enabled";
             }
             else
             {
                 texthumidity.text = "humidity null";        
             }
     
             if (ambient != null)
             {
                 InputSystem.EnableDevice(ambient);
                 texttemp.text = "ambient null";
             }
             else
             {
                 texttemp.text = "ambient null";
             }
     
             if (step != null)
             {
                 InputSystem.EnableDevice(step);
                 textstepcount.text = "step enabled";
             }
             else
             {
                 textstepcount.text = "step null";
             }
         }
     
         void Update()
         {
              if (accelerometer != null)
              {
                  textaccel.text = $"accelerometer: {accelerometer.acceleration.ReadValue()}";
              }

              if (gyroscope != null)
              {
                  textgyro.text = $"gyroscope: {gyroscope.angularVelocity.ReadValue()}";
              }

              if (gravity != null)
              {
                  textgravity.text = $"gravity: {gravity.gravity.ReadValue()}";
              }

              if (attitude != null)
              {
                  textattitude.text = $"attitude: {attitude.attitude.ReadValue()}";
              }

              if (linearaccel != null)
              {
                  textlinearaccel.text = $"linearaccel: {linearaccel.acceleration.ReadValue()}";
              }

              if (magfield != null)
              {
                  textmagfield.text = $"magfield: {magfield.magneticField.ReadValue()}";
              }

              if (lightsens != null)
              {
                  textlight.text = $"lightsens: {lightsens.lightLevel.ReadValue()}";
              }

              if (pressure != null)
              {
                  textpressure.text = $"pressure: {pressure.atmosphericPressure.ReadValue()}";
              }

              if (step != null)
              {
                  textstepcount.text = $"step: {step.stepCounter.ReadValue()}";
              }
         }
     }
 }
