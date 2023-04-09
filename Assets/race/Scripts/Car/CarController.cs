using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class CarController : MonoBehaviourValidated
{
    public struct InputData
    {
        public float steer;
        public float accelerate;
        public float brake;
        public Vector2 camera;

        public Vector2 drift;

        public int gear;
    }

    [SerializeField, Child] public Wheel[] wheels;

    [ReadOnly] public Car car;

    public InputData inputData;

    private void FixedUpdate()
    {
        float gripFactor = 0;
        float speedRatio = 0;
        foreach (Wheel wheel in wheels)
        {
            Wheel.WheelData wheelData = wheel.CalculateWheelData();
            gripFactor += wheelData.gripFactor;
            speedRatio += wheelData.speedRatio;
        }
        gripFactor /= wheels.Length;
        speedRatio /= wheels.Length;

        float directionDot = Vector3.Dot(transform.forward, car.RB.velocity.normalized);
        if (gripFactor < car.config.gripToDriftThreshold && inputData.drift.x <= 0)
        {
            inputData.drift.x = car.RB.velocity.magnitude;
            inputData.drift.y = car.RB.velocity.magnitude;
        }
        if (gripFactor >= car.config.gripToDriftThreshold || speedRatio < 0.35f || directionDot > 0.995f)
        {
            inputData.drift.x = 0;
            inputData.drift.y = 0;
        }

        if (inputData.drift.x > 0)
        {
            float angleCos = car.config.driftRotationScaling.Evaluate(Mathf.Clamp01(directionDot));
            car.RB.rotation *= Quaternion.AngleAxis(inputData.steer * car.config.driftCarAngleModifier * angleCos * Time.fixedDeltaTime, transform.up);
        }
    }

    public void Steer(CallbackContext context)
    {
        inputData.steer = context.ReadValue<float>();
    }

    public void Accelerate(CallbackContext context)
    {
        inputData.accelerate = context.ReadValue<float>();
    }

    public void Brake(CallbackContext context)
    {
        inputData.brake = context.ReadValue<float>();
    }

    public void Camera(CallbackContext context)
    {
        inputData.camera = context.ReadValue<Vector2>();
    }

    public void GearUp(CallbackContext context)
    {
        if (context.performed)
        {
            if (inputData.gear < car.config.motorTorqueResponseCurve.Count - 1) inputData.gear++;
        }
    }

    public void GearDown(CallbackContext context)
    {
        if (context.performed)
        {
            if (inputData.gear > 0) inputData.gear--;
        }
    }
}
