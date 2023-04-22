using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class Car : ValidatedMonoBehaviour
{
    [System.Serializable]
    public struct InputData
    {
        public float steer;
        public float accelerate;
        public float brake;
        public Vector2 camera;

        public float drift;

        public int gear;
    }

    [SerializeField, Child] public Wheel[] wheels;
    [SerializeField, Self] public Rigidbody RB;

    [SerializeField, Anywhere] public VisualEffect smokePrefab;

    [ReadOnly] public CarController controller;
    [ReadOnly] public CarTemplate template;
    [ReadOnly] public CarModel model;
    [ReadOnly] public CarConfig config;

    [ReadOnly] public InputData inputData;

    public bool automaticTransmission;

    public void CarSetup(CarController carController, CarTemplate carTemplate, CarModel carModel, CarConfig carConfig)
    {
        config = carConfig;

        RB.mass = config.carMass;
        RB.centerOfMass = config.centerOfMass;

        inputData.gear = 1;

        model = carModel;

        controller = carController;
        controller.car = this;

        foreach (Wheel wheel in wheels)
        {
            wheel.transform.position = model.wheelPositions[(int)wheel.wheelType].position;
            wheel.transform.rotation = transform.rotation;
            wheel.SetupSmoke(smokePrefab);
        }

        template = carTemplate;
        if (template != null)
        {
            template.virtualCamera.Follow = transform;
            template.virtualCamera.LookAt = transform;
        }
    }

    public void RaceSetup()
    {
        LapManager.Instance.checkpointTracker.Add(this, new LapManager.LapTracker(Time.timeSinceLevelLoad, 1, 0));

        StartingGridPoint gridPoint = LapManager.Instance.AllocateGridPoint();
        RB.position = gridPoint.transform.position;
        RB.rotation = gridPoint.transform.rotation;
    }

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
        float gripFactor = 0;
        float speedRatio = 0;
        bool grounded = false;
        foreach (Wheel wheel in wheels)
        {
            Wheel.WheelData wheelData = wheel.CalculateWheelData();
            gripFactor += wheelData.gripFactor;
            speedRatio += wheelData.topSpeedRatio;
            grounded |= wheel.Grounded;
        }
        gripFactor /= wheels.Length;
        speedRatio /= wheels.Length;

        float directionDot = Vector3.Dot(transform.forward, RB.velocity.normalized);
        if (gripFactor < config.gripToDriftThreshold && inputData.drift <= 0)
        {
            inputData.drift = RB.velocity.magnitude;
        }
        if (gripFactor >= config.gripToDriftThreshold || speedRatio < 0.35f || directionDot > 0.995f)
        {
            inputData.drift = 0;
        }

        if (inputData.drift > 0)
        {
            float angleCos = config.driftRotationScaling.Evaluate(Mathf.Clamp01(directionDot));
            RB.rotation *= Quaternion.AngleAxis(inputData.steer * config.driftCarAngleModifier * angleCos * Time.fixedDeltaTime, transform.up);

            inputData.drift = Mathf.Lerp(inputData.drift, RB.velocity.magnitude, (1 + inputData.brake) * config.driftAdjustSpeed * Time.fixedDeltaTime);
        }

        RB.drag = grounded ? 0 : 0.6f;
        RB.angularDrag = grounded ? 0.05f : 0.5f;
    }
}
