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
        public bool slipstream;
        public float rocketStart;

        public int gear;
    }

    [SerializeField, Child] public Wheel[] wheels;
    [SerializeField, Self] public Rigidbody RB;

    [SerializeField, Anywhere] public VisualEffect smokePrefab;
    [SerializeField, Anywhere] public VisualEffect dirtPrefab;
    [SerializeField, Anywhere] public VisualEffect slipstreamPrefab;

    [SerializeField, Child] public BoxCollider slipstreamCollider;

    [ReadOnly] public ICarController controller;
    [ReadOnly] public CarModel model;
    [ReadOnly] public CarConfig config;

    [ReadOnly] public StartingGridPoint gridPoint;

    [ReadOnly] public InputData inputData;

    public bool automaticTransmission;

    private VisualEffect slipstreamEffect;

    private float slipstreamColliderOriginalSize;

    private void Start()
    {
        slipstreamColliderOriginalSize = slipstreamCollider.size.z;
    }

    public void CarSetup(ICarController carController, CarModel carModel, CarConfig carConfig)
    {
        config = carConfig;

        RB.mass = config.carMass;
        RB.centerOfMass = config.centerOfMass;

        model = carModel;

        controller = carController;
        controller.Car = this;
        if (controller.VirtualCamera != null)
        {
            controller.VirtualCamera.Follow = transform;
            controller.VirtualCamera.LookAt = transform;
        }

        foreach (Wheel wheel in wheels)
        {
            wheel.transform.position = model.wheelPositions[(int)wheel.wheelType].position;
            wheel.transform.rotation = transform.rotation;
            wheel.SetupParticles(smokePrefab, dirtPrefab);
        }

        if (slipstreamEffect == null) slipstreamEffect = Instantiate(slipstreamPrefab, transform);
    }

    public void PlaceInStartingGrid()
    {
        gridPoint = LapManager.Instance.AllocateGridPoint();
        RB.position = gridPoint.transform.position;
        RB.rotation = gridPoint.transform.rotation;
    }

    public float GetGearRatio()
    {
        if (inputData.gear == 0)
        {
            return inputData.accelerate;
        }
        else
        {
            float forwardComponent = Vector3.Dot(transform.forward, RB.velocity);
            float forwardRatio = forwardComponent / config.TopSpeed(this);
            AnimationCurve gearCurve = config.motorTorqueResponseCurve[inputData.gear + 1];

            Keyframe first = gearCurve.keys.First(k => k.value >= 0);
            Keyframe last = gearCurve.keys.Last(k => k.value >= 0);

            float rpmRatio = (forwardRatio - first.time) / (last.time - first.time);

            return Mathf.Clamp01(inputData.gear == -1 ? 1 - rpmRatio : rpmRatio);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CheckpointCollider>() is CheckpointCollider checkpoint)
        {
            LapManager.Instance.UpdateCheckpoint(this, checkpoint.order);
        }

        if (!inputData.slipstream && other.gameObject.layer == LayerMask.NameToLayer("SlipstreamArea"))
        {
            inputData.slipstream = true;
            slipstreamEffect.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (inputData.slipstream && other.gameObject.layer == LayerMask.NameToLayer("SlipstreamArea"))
        {
            inputData.slipstream = false;
            slipstreamEffect.Stop();
        }
    }

    private void Update()
    {
        if (!RaceManager.Instance.RaceStarting && automaticTransmission && inputData.gear > 0)
        {
            float ratio = GetGearRatio();
            if (inputData.gear < config.motorTorqueResponseCurve.Count - 2 && ratio > config.automaticGearLimits.y) inputData.gear++;
            if (inputData.gear > 1 && ratio < config.automaticGearLimits.x) inputData.gear--;
        }

        if (RaceManager.Instance.RaceRunning && inputData.rocketStart > 0)
        {
            inputData.rocketStart -= Time.deltaTime;
            if (inputData.rocketStart < 0) inputData.rocketStart = 0;
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

        float speed = RB.velocity.magnitude;

        float directionDot = Vector3.Dot(transform.forward, RB.velocity.normalized);
        if (inputData.accelerate < 0.01f && inputData.brake > 0.99f && Mathf.Abs(inputData.steer) > 0.9f && inputData.drift <= 0)
        {
            inputData.drift = speed;
        }
        else if ((Vector3.Dot(RB.velocity, transform.forward) > 0.99f && Mathf.Abs(inputData.steer) < 0.1f) || speedRatio < 0.35f)
        {
            inputData.drift = 0;
        }

        if (inputData.drift > 0)
        {
            RB.rotation *= Quaternion.AngleAxis(inputData.steer * 120 * Mathf.Clamp01(directionDot) * Time.fixedDeltaTime, transform.up);

            float adjustScale = (2 * inputData.brake) + (inputData.accelerate);
            inputData.drift = Mathf.Lerp(inputData.drift, speed, 4 * adjustScale * Time.fixedDeltaTime);
        }

        RB.drag = grounded ? 0 : 0.6f;
        RB.angularDrag = grounded ? 0.05f : 0.5f;

        slipstreamCollider.size = new Vector3(slipstreamCollider.size.x, slipstreamCollider.size.y, slipstreamColliderOriginalSize * speed / config.TopSpeed(this));
        slipstreamCollider.center = new Vector3(0, 0, (slipstreamColliderOriginalSize - slipstreamCollider.size.z) / 2);
    }
}
