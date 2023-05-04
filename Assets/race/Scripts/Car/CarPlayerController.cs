using Cinemachine;
using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class CarPlayerController : ValidatedMonoBehaviour, ICarController
{
    public CinemachineVirtualCamera VirtualCamera => virtualCamera;
    public GameObject GameObject => gameObject;
    public Car Car { get; set; }

    [SerializeField, Child] public CinemachineVirtualCamera virtualCamera;

    public void Steer(CallbackContext context)
    {
        Car.inputData.steer = context.ReadValue<float>();
    }

    public void Accelerate(CallbackContext context)
    {
        Car.inputData.accelerate = context.ReadValue<float>();
    }

    public void Brake(CallbackContext context)
    {
        Car.inputData.brake = context.ReadValue<float>();
    }

    public void Camera(CallbackContext context)
    {
        Car.inputData.camera = context.ReadValue<Vector2>();
    }

    public void GearUp(CallbackContext context)
    {
        if (context.performed)
        {
            if (RaceManager.Instance.RaceRunning && Car.inputData.gear < Car.config.motorTorqueResponseCurve.Count - 2) Car.inputData.gear++;
        }
    }

    public void GearDown(CallbackContext context)
    {
        if (context.performed)
        {
            if (RaceManager.Instance.RaceRunning && Car.inputData.gear >= 0) Car.inputData.gear--;
        }
    }
}
