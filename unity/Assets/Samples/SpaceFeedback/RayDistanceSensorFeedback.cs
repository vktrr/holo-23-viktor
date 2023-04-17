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
                var speed = Mathf.InverseLerp(2, 0.5f, raycastTest.hitInfo.distance);
                lastInputDevice.SendMotorSpeed(speed);
            }
            else
            {

                lastInputDevice.SendMotorSpeed(0);
            }
        }
    }
}
