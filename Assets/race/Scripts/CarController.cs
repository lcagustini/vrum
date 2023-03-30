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

        public int gear;
    }

    [SerializeField, Self] public Rigidbody RB;

    public CarConfig config;

    public InputData inputData;

    public bool automaticTransmission;

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
        if (automaticTransmission && inputData.accelerate > 0.1f && inputData.gear > 0)
        {
            Vector3 velocity = RB.GetPointVelocity(transform.position);

            float forwardComponent = Vector3.Dot(transform.forward, velocity);
            float forwardRatio = forwardComponent / config.topSpeed;

            float max = config.motorTorqueResponseCurve[0].Evaluate(forwardRatio);
            int maxIndex = 0;

            for (int i = 1; i < config.motorTorqueResponseCurve.Count; i++)
            {
                float value = config.motorTorqueResponseCurve[i].Evaluate(forwardRatio);
                if (value > max)
                {
                    maxIndex = i;
                    max = value;
                }
            }

            inputData.gear = maxIndex;
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
            if (inputData.gear < config.motorTorqueResponseCurve.Count - 1) inputData.gear++;
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
