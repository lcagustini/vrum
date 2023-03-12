using SceneRefAttributes;
using System.Collections;
using System.Collections.Generic;
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
    }

    [SerializeField, Self] public Rigidbody RB;
    [SerializeField, Child] public new Camera camera;
    private Vector3 cameraOriginalPosition;
    private Quaternion cameraOriginalRotation;

    public float topSpeed;

    public AnimationCurve motorTorqueResponseCurve;
    public float motorMaxTorque;

    public AnimationCurve brakeResponseCurve;

    public InputData inputData;

    private void Awake()
    {
        cameraOriginalPosition = camera.transform.localPosition;
        cameraOriginalRotation = camera.transform.localRotation;
    }

    private void Update()
    {
        Quaternion rotation = Quaternion.AngleAxis(inputData.camera.x * 90, transform.up);
        camera.transform.localPosition = rotation * cameraOriginalPosition;
        camera.transform.localRotation = rotation * cameraOriginalRotation;
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
}
