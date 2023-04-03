using SceneRefAttributes;
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

        public float drift;

        public int gear;
    }

    [SerializeField, Self] public Rigidbody RB;

    public CarConfig config;

    public InputData inputData;

    public bool automaticTransmission;

    public float GetGearRatio()
    {
        float forwardComponent = Vector3.Dot(transform.forward, RB.velocity);
        float forwardRatio = forwardComponent / config.topSpeed;
        AnimationCurve gearCurve = config.motorTorqueResponseCurve[inputData.gear];

        Keyframe first = gearCurve.keys.First(k => k.value >= 0);
        Keyframe last = gearCurve.keys.Last(k => k.value >= 0);

        float rpmRatio = (forwardRatio - first.time) / (last.time - first.time);

        return Mathf.Clamp01(inputData.gear == 0 ? 1 - rpmRatio : rpmRatio);
    }

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
        if (automaticTransmission && inputData.gear > 0)
        {
            float ratio = GetGearRatio();
            if (inputData.gear < config.motorTorqueResponseCurve.Count - 1 && ratio > config.automaticGearLimits.y) inputData.gear++;
            if (inputData.gear > 1 && ratio < config.automaticGearLimits.x) inputData.gear--;
        }
    }

    private void FixedUpdate()
    {
        if (inputData.drift > 0)
        {
            float angleCos = Mathf.Clamp01(Vector3.Dot(RB.velocity.normalized, transform.forward));
            RB.rotation *= Quaternion.AngleAxis(inputData.steer * config.driftCarAngleModifier * angleCos * Time.fixedDeltaTime, transform.up);
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

    public void Drift(CallbackContext context)
    {
        inputData.drift = context.performed ? RB.velocity.magnitude : 0;
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
