using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class Car : MonoBehaviourValidated
{
    [ReadOnly] public CarController controller;
    [ReadOnly] public CarTemplate template;
    [ReadOnly] public CarModel model;
    [ReadOnly] public CarConfig config;

    [SerializeField, Anywhere] public VisualEffect smokePrefab;

    [SerializeField, Self] public Rigidbody RB;

    public bool automaticTransmission;

    public void Setup(CarController carController, CarTemplate carTemplate, CarModel carModel, CarConfig carConfig)
    {
        config = carConfig;

        RB.mass = config.carMass;
        RB.centerOfMass = config.centerOfMass;

        model = carModel;

        controller = carController;
        controller.car = this;
        foreach (Wheel wheel in controller.wheels)
        {
            wheel.transform.position = model.wheelPositions[(int)wheel.wheelType].position;
            wheel.transform.rotation = transform.rotation;
            wheel.SetupSmoke(smokePrefab);
        }

        template = carTemplate;
        template.virtualCamera.Follow = transform;
        template.virtualCamera.LookAt = transform;

        LapManager.Instance.checkpointTracker.Add(this, new LapManager.LapTracker(Time.timeSinceLevelLoad, 1, 0));

        CheckpointCollider checkpointCollider = LapManager.Instance.GetLastCollider();
        RB.position = checkpointCollider.transform.position;
        RB.rotation = checkpointCollider.transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CheckpointCollider>() is CheckpointCollider checkpoint)
        {
            LapManager.Instance.UpdateCheckpoint(this, checkpoint.order);
        }
    }

    public float GetGearRatio()
    {
        float forwardComponent = Vector3.Dot(transform.forward, RB.velocity);
        float forwardRatio = forwardComponent / config.topSpeed;
        AnimationCurve gearCurve = config.motorTorqueResponseCurve[controller.inputData.gear];

        Keyframe first = gearCurve.keys.First(k => k.value >= 0);
        Keyframe last = gearCurve.keys.Last(k => k.value >= 0);

        float rpmRatio = (forwardRatio - first.time) / (last.time - first.time);

        return Mathf.Clamp01(controller.inputData.gear == 0 ? 1 - rpmRatio : rpmRatio);
    }

    private void Update()
    {
        if (automaticTransmission && controller.inputData.gear > 0)
        {
            float ratio = GetGearRatio();
            if (controller.inputData.gear < config.motorTorqueResponseCurve.Count - 1 && ratio > config.automaticGearLimits.y) controller.inputData.gear++;
            if (controller.inputData.gear > 1 && ratio < config.automaticGearLimits.x) controller.inputData.gear--;
        }
    }
}
