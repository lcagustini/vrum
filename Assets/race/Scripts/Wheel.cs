using KBCore.Refs;
using Unity.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class Wheel : ValidatedMonoBehaviour
{
    public struct WheelData
    {
        public Vector3 velocity;
        public float speed;
        public float topSpeedRatio;

        public float forwardComponent;
        public float forwardRatio;
        public float topSpeedforwardRatio;

        public float sidewaysComponent;
        public float sidewaysRatio;

        public float upComponent;

        public float motorTorqueFactor;
        public float gripFactor;
    }

    public enum GroundType
    {
        Dirt,
        Road
    }

    [SerializeField, Parent] private Car car;

    private VisualEffect driftSmoke;
    private bool isSmokePlaying;

    private VisualEffect driftDirt;
    private bool isDirtPlaying;

    [SerializeField] public WheelType wheelType;

    private float springLength;
    public bool Grounded { get; private set; }

    public Vector3 WheelPosition => transform.position + (springLength * -transform.up);
    public Vector3 GroundPoint => transform.position + ((springLength + car.config.wheelRadius) * -transform.up);

    private bool IsMotorWheel => ((wheelType == WheelType.BackLeft || wheelType == WheelType.BackRight) && (car.config.drivetrain == Drivetrain.RearWheelDrive || car.config.drivetrain == Drivetrain.AllWheelDrive)) || ((wheelType == WheelType.FrontLeft || wheelType == WheelType.FrontRight) && (car.config.drivetrain == Drivetrain.FrontWheelDrive || car.config.drivetrain == Drivetrain.AllWheelDrive));
    private bool IsSteeringWheel => wheelType == WheelType.FrontLeft || wheelType == WheelType.FrontRight;

    public GroundType groundType;

    private Vector3 ApplySuspensionForce(WheelData wheelData)
    {
        float springForce = car.config.springStrength * (car.config.springRestDistance - springLength);
        float dampingForce = car.config.dampingStrength * wheelData.upComponent;

        Vector3 forceVector = (springForce - dampingForce) * transform.up;

        Debug.DrawLine(transform.position, transform.position + (forceVector / car.RB.mass), Color.magenta);
        car.RB.AddForceAtPosition(forceVector, transform.position);

        return forceVector;
    }

    private Vector3 ApplySteeringForce(WheelData wheelData)
    {
        float gripForce = wheelData.gripFactor * wheelData.sidewaysComponent * car.config.wheelMass / Time.fixedDeltaTime;

        Vector3 forceVector = -gripForce * transform.right;

        Debug.DrawLine(transform.position, transform.position + (forceVector / car.RB.mass), Color.magenta);
        car.RB.AddForceAtPosition(forceVector, transform.position);

        return forceVector;
    }

    private Vector3 ApplyAccelerationForce(WheelData wheelData)
    {
        if (IsMotorWheel)
        {
            float motorTorque = (car.inputData.rocketStart > 0 ? 1.5f : 1.0f) * wheelData.motorTorqueFactor * car.config.motorMaxTorque;
            Vector3 forceVector = wheelData.gripFactor * motorTorque * transform.forward;

            Debug.DrawLine(transform.position, transform.position + (forceVector / car.RB.mass), Color.magenta);
            car.RB.AddForceAtPosition(forceVector, transform.position);

            return forceVector;
        }

        return Vector3.zero;
    }

    private Vector3 ApplyDriftForce(WheelData wheelData)
    {
        float speedDiff = (1 - car.inputData.brake) * (car.inputData.drift - wheelData.speed) / car.config.TopSpeed(car);
        float driftFactor = car.inputData.drift > 0 ? Mathf.Clamp(100 * speedDiff, -1, 1) : 0;
        float driftTorque = driftFactor * car.config.motorMaxTorque;

#if false
        Vector3 halfwayVector = (wheelData.velocity.normalized + transform.forward).normalized;
        halfwayVector = 2 * transform.forward * Vector3.Dot(transform.forward, halfwayVector) - halfwayVector;
        Vector3 forceVector = driftTorque * halfwayVector;
#else
        float steer = car.config.driftSteerLimit * Vector3.Dot(wheelData.velocity.normalized, car.transform.forward) * Mathf.Abs(car.inputData.steer);
        Vector3 halfwayVector = Vector3.Slerp(transform.forward, car.inputData.steer > 0 ? transform.right : -transform.right, steer);
        Vector3 forceVector = driftTorque * halfwayVector;
#endif

        Debug.DrawLine(transform.position, transform.position + (forceVector / car.RB.mass), Color.magenta);
        car.RB.AddForceAtPosition(forceVector, transform.position);

        return forceVector;
    }

    private Vector3 ApplyBrakeForce(float input, WheelData wheelData)
    {
        float brakeFactor = input * car.config.brakeResponseCurve.Evaluate(wheelData.topSpeedforwardRatio);
        float brakeForce = -brakeFactor * wheelData.forwardComponent * car.config.wheelMass / Time.fixedDeltaTime;

        float gripFactor = car.inputData.drift > 0 ? 1 : wheelData.gripFactor;
        Vector3 forceVector = brakeForce * gripFactor * transform.forward;

        Debug.DrawLine(transform.position, transform.position + (forceVector / car.RB.mass), Color.red);
        car.RB.AddForceAtPosition(forceVector, transform.position);

        return forceVector;
    }

    public WheelData CalculateWheelData()
    {
        WheelData wheelData = new WheelData();

        wheelData.velocity = car.RB.GetPointVelocity(transform.position);
        wheelData.speed = wheelData.velocity.magnitude;
        wheelData.topSpeedRatio = wheelData.speed / car.config.TopSpeed(car);

        wheelData.forwardComponent = Vector3.Dot(transform.forward, wheelData.velocity);
        wheelData.forwardRatio = wheelData.speed == 0 ? 0 : Mathf.Abs(wheelData.forwardComponent / wheelData.speed);
        wheelData.topSpeedforwardRatio = wheelData.forwardComponent / car.config.TopSpeed(car);

        wheelData.sidewaysComponent = Vector3.Dot(transform.right, wheelData.velocity);
        wheelData.sidewaysRatio = wheelData.speed == 0 ? 0 : Mathf.Abs(wheelData.sidewaysComponent / wheelData.speed);

        wheelData.upComponent = Vector3.Dot(transform.up, wheelData.velocity);

        wheelData.motorTorqueFactor = car.config.steeringAccelerationFactorCurves[(int)wheelType].Evaluate(wheelData.sidewaysRatio) * Mathf.Clamp(car.inputData.accelerate * car.config.motorTorqueResponseCurve[car.inputData.gear + 1].Evaluate(wheelData.topSpeedforwardRatio), -1.0f, 1.0f);

        wheelData.gripFactor = car.config.speedGripFactorCurves[(int)wheelType].Evaluate(wheelData.topSpeedRatio) * car.config.sidewaysGripFactorCurves[(int)wheelType].Evaluate(wheelData.sidewaysRatio);
        if (wheelData.speed < 0.001f) wheelData.gripFactor = Mathf.Clamp01(10 * wheelData.gripFactor);

        return wheelData;
    }

    public void SetupParticles(VisualEffect smokePrefab, VisualEffect dirtPrefab)
    {
        if (driftSmoke == null) driftSmoke = Instantiate(smokePrefab, transform);
        if (driftDirt == null) driftDirt = Instantiate(dirtPrefab, transform);
    }

    private void FixedUpdate()
    {
        WheelData wheelData = CalculateWheelData();

        if (IsSteeringWheel)
        {
            float turnAmountFactor = car.config.wheelForwardTurnModifier.Evaluate(wheelData.topSpeedforwardRatio) + car.config.wheelSidewaysTurnModifier.Evaluate(wheelData.sidewaysRatio);
            float turnAmount = turnAmountFactor * car.config.wheelMaxTurnDegrees;

            transform.localRotation = Quaternion.Euler(0, turnAmount * car.inputData.steer, 0);
        }

        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hitInfo, car.config.springMaxTravel + car.config.wheelRadius, LayerMask.GetMask("DirtCollider", "RoadCollider")))
        {
            switch (LayerMask.LayerToName(hitInfo.collider.gameObject.layer))
            {
                case "DirtCollider":
                    groundType = GroundType.Dirt;
                    break;
                case "RoadCollider":
                    groundType = GroundType.Road;
                    break;
            }

            if (groundType == GroundType.Dirt)
            {
                wheelData.gripFactor *= 0.6f;
                ApplyBrakeForce(0.2f, wheelData);
            }

            Grounded = true;

            springLength = hitInfo.distance - car.config.wheelRadius;

            ApplySuspensionForce(wheelData);
            if (!RaceManager.Instance.RaceStarting)
            {
                ApplySteeringForce(wheelData);
                ApplyAccelerationForce(wheelData);
                ApplyBrakeForce(car.inputData.brake, wheelData);
                ApplyDriftForce(wheelData);
            }

            if (car.inputData.accelerate < MathHelper.epsilon) ApplyBrakeForce(0.05f, wheelData);
        }
        else
        {
            Grounded = false;

            springLength = car.config.springMaxTravel;
        }

        if (Grounded && car.inputData.drift > 0)
        {
            switch (groundType)
            {
                case GroundType.Dirt:
                    if (!isDirtPlaying)
                    {
                        driftDirt.Play();
                        isDirtPlaying = true;
                    }
                    if (isSmokePlaying)
                    {
                        driftSmoke.Stop();
                        isSmokePlaying = false;
                    }
                    break;
                case GroundType.Road:
                    if (!isSmokePlaying)
                    {
                        driftSmoke.Play();
                        isSmokePlaying = true;
                    }
                    if (isDirtPlaying)
                    {
                        driftDirt.Stop();
                        isDirtPlaying = false;
                    }
                    break;
            }
        }
        else
        {
            if (isSmokePlaying || isDirtPlaying)
            {
                driftSmoke.Stop();
                driftDirt.Stop();
                isSmokePlaying = false;
                isDirtPlaying = false;
            }
        }
    }

    private void Update()
    {
        car.model.wheelVisuals[(int)wheelType].transform.position = Vector3.MoveTowards(car.model.wheelVisuals[(int)wheelType].transform.position, WheelPosition, 1f * Time.deltaTime);
        car.model.wheelVisuals[(int)wheelType].transform.rotation = Quaternion.RotateTowards(car.model.wheelVisuals[(int)wheelType].transform.rotation, transform.rotation, 90 * Time.deltaTime);

        driftSmoke.transform.position = GroundPoint;
        driftDirt.transform.position = GroundPoint;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, WheelPosition);

            WheelData wheelData = CalculateWheelData();
            Gizmos.color = Color.Lerp(Color.red, Color.green, wheelData.gripFactor);

            Gizmos.DrawSphere(WheelPosition, car.config.wheelRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + (car.config.springRestDistance * -transform.up));
        }
    }
#endif
}
