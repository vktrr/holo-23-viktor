using UnityEngine;
using UnityEngine.InputSystem;

public class RayDistanceSensorFeedback : MonoBehaviour
{
    void Update()
    {
        var lastInputDevice = InputSystem.GetDevice<Esp32InputDevice>();
        var raycastTest = GetComponent<RayTest>();

        if(lastInputDevice!= null)
        {
            if (raycastTest.isHit)
            {
                var speed0 = Mathf.InverseLerp(2, 0.5f, raycastTest.hitInfo.distance)*
                             Mathf.InverseLerp(.2f, 0.6f, raycastTest.hitInfo.distance);;
                var speed1 = Mathf.InverseLerp(.6f, 0.2f, raycastTest.hitInfo.distance);
                lastInputDevice.SendMotorSpeed(0,speed0);
                lastInputDevice.SendMotorSpeed(1,speed1);
            }
            else
            {

                lastInputDevice.SendMotorSpeed(0,0);
                lastInputDevice.SendMotorSpeed(1,0);
            }
        }
    }
}
