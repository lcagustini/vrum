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

        public bool powerTurn;
    }

    [SerializeField, Self] public Rigidbody RB;

    public CarConfig config;

    public InputData inputData;

    private void Start()
    {
        LapManager.Instance.checkpointTracker.Add(this, new LapManager.LapTracker(Time.timeSinceLevelLoad, 1, 0));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CheckpointCollider>() is CheckpointCollider checkpoint)
        {
            LapManager.Instance.UpdateCheckpoint(this, checkpoint.order);
        }
    }

    private void Update()
    {
        if (!inputData.powerTurn && inputData.accelerate > 0.5f && inputData.brake > 0.5f && Mathf.Abs(inputData.steer) > 0.5f) inputData.powerTurn = true;
        if (inputData.powerTurn && (Mathf.Abs(inputData.steer) < 0.1f || inputData.accelerate < 0.1f)) inputData.powerTurn = false;
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
