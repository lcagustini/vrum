using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class CarController : MonoBehaviourValidated
{
    [ReadOnly] public Car car;

    public void Steer(CallbackContext context)
    {
        car.inputData.steer = context.ReadValue<float>();
    }

    public void Accelerate(CallbackContext context)
    {
        car.inputData.accelerate = context.ReadValue<float>();
    }

    public void Brake(CallbackContext context)
    {
        car.inputData.brake = context.ReadValue<float>();
    }

    public void Camera(CallbackContext context)
    {
        car.inputData.camera = context.ReadValue<Vector2>();
    }

    public void GearUp(CallbackContext context)
    {
        if (context.performed)
        {
            if (car.inputData.gear < car.config.motorTorqueResponseCurve.Count - 1) car.inputData.gear++;
        }
    }

    public void GearDown(CallbackContext context)
    {
        if (context.performed)
        {
            if (car.inputData.gear > 0) car.inputData.gear--;
        }
    }
}
