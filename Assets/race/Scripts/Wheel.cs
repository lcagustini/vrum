using SceneRefAttributes;
using Unity.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class Wheel : MonoBehaviourValidated
{
    public struct WheelData
    {
        public Vector3 velocity;
        public float speed;
        public float speedRatio;

        public float forwardComponent;
        public float forwardRatio;

        public float sidewaysComponent;
        public float sidewaysRatio;

        public float upComponent;

        public float gripFactor;
    }

    [SerializeField, Parent] private CarController car;

    [SerializeField, Child(Flag.Optional)] private VisualEffect driftSmoke;
    private bool isSmokePlaying;

    [SerializeField, Anywhere] private Transform wheelVisual;

    [SerializeField] private WheelType wheelType;

    [SerializeField] private float smokeThreshold;

    private float springLength;
    private bool grounded;

    public Vector3 WheelPosition => transform.position + (springLength * -transform.up);

    private bool IsMotorWheel => ((wheelType == WheelType.BackLeft || wheelType == WheelType.BackRight) && (car.config.drivetrain == Drivetrain.RearWheelDrive || car.config.drivetrain == Drivetrain.AllWheelDrive)) || ((wheelType == WheelType.FrontLeft || wheelType == WheelType.FrontRight) && (car.config.drivetrain == Drivetrain.FrontWheelDrive || car.config.drivetrain == Drivetrain.AllWheelDrive));
    private bool IsSteeringWheel => wheelType == WheelType.FrontLeft || wheelType == WheelType.FrontRight;

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
            float motorTorqueFactor = car.config.steeringAccelerationFactorCurves[(int)wheelType].Evaluate(wheelData.sidewaysRatio) * Mathf.Clamp(car.inputData.accelerate * car.config.motorTorqueResponseCurve[car.inputData.gear].Evaluate(wheelData.forwardRatio), -1.0f, 1.0f);
            float motorTorque = motorTorqueFactor * car.config.motorMaxTorque;

            Vector3 forceVector = wheelData.gripFactor * motorTorque * transform.forward;

            Debug.DrawLine(transform.position, transform.position + (forceVector / car.RB.mass), Color.magenta);
            car.RB.AddForceAtPosition(forceVector, transform.position);

            return forceVector;
        }

        return Vector3.zero;
    }

    private Vector3 ApplyDriftForce(WheelData wheelData)
    {
        float driftFactor = car.config.driftTorqueModifier * car.config.driftAccelerationFactorCurves[(int)wheelType].Evaluate((car.inputData.drift.y - wheelData.speed) / (car.inputData.drift.y + wheelData.speed));
        float driftTorque = driftFactor * car.config.motorMaxTorque;

        Vector3 halfwayVector = (wheelData.velocity.normalized + transform.forward).normalized;
        Vector3 forceVector = wheelData.gripFactor * driftTorque * halfwayVector;

        Debug.DrawLine(transform.position, transform.position + (forceVector / car.RB.mass), Color.magenta);
        car.RB.AddForceAtPosition(forceVector, transform.position);

        return forceVector;
    }

    private Vector3 ApplyBrakeForce(float input, WheelData wheelData)
    {
        float brakeFactor = input * car.config.brakeResponseCurve.Evaluate(wheelData.forwardRatio);
        float brakeForce = -brakeFactor * wheelData.forwardComponent * car.config.wheelMass / Time.fixedDeltaTime;

        Vector3 forceVector = brakeForce * wheelData.gripFactor * transform.forward;

        Debug.DrawLine(transform.position, transform.position + (forceVector / car.RB.mass), Color.magenta);
        car.RB.AddForceAtPosition(forceVector, transform.position);

        return forceVector;
    }

    public WheelData CalculateWheelData()
    {
        WheelData wheelData = new WheelData();

        wheelData.velocity = car.RB.GetPointVelocity(transform.position);
        wheelData.speed = wheelData.velocity.magnitude;
        wheelData.speedRatio = wheelData.speed / car.config.topSpeed;

        wheelData.forwardComponent = Vector3.Dot(transform.forward, wheelData.velocity);
        wheelData.forwardRatio = wheelData.forwardComponent / car.config.topSpeed;

        wheelData.sidewaysComponent = Vector3.Dot(transform.right, wheelData.velocity);
        wheelData.sidewaysRatio = wheelData.speed == 0 ? 0 : Mathf.Abs(wheelData.sidewaysComponent / wheelData.speed);

        wheelData.upComponent = Vector3.Dot(transform.up, wheelData.velocity);

        wheelData.gripFactor = car.config.speedGripFactorCurves[(int)wheelType].Evaluate(wheelData.speedRatio) * car.config.sidewaysGripFactorCurves[(int)wheelType].Evaluate(wheelData.sidewaysRatio) * car.config.brakeGripLossCurves[(int)wheelType].Evaluate(car.inputData.brake);

        return wheelData;
    }

    private void FixedUpdate()
    {
        WheelData wheelData = CalculateWheelData();

        if (IsSteeringWheel)
        {
            float turnAmountFactor = car.config.wheelForwardTurnModifier.Evaluate(wheelData.forwardRatio) + car.config.wheelSidewaysTurnModifier.Evaluate(wheelData.sidewaysRatio);
            float turnAmount = turnAmountFactor * car.config.wheelMaxTurnDegrees;

            transform.localRotation = Quaternion.Euler(0, turnAmount * car.inputData.steer, 0);
        }

        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hitInfo, car.config.springMaxTravel + car.config.wheelRadius, LayerMask.GetMask("GroundCollider")))
        {
            grounded = true;

            springLength = hitInfo.distance - car.config.wheelRadius;

            Vector3 planarForce = Vector3.zero;

            ApplySuspensionForce(wheelData);
            planarForce += ApplySteeringForce(wheelData);
            planarForce += ApplyAccelerationForce(wheelData);
            planarForce += ApplyBrakeForce(car.inputData.brake, wheelData);
            ApplyDriftForce(wheelData);

            Vector3 velocityChange = planarForce * Time.fixedDeltaTime / car.RB.mass;
            if (car.inputData.drift.x > 0)
            {
                float speedDiff = 2 * Mathf.Abs(car.inputData.drift.x - car.inputData.drift.y) / (car.inputData.drift.x + car.inputData.drift.y);
                float speedDot = Vector3.Dot(velocityChange, transform.forward);
                float negativeModifier = 1 - (speedDiff / car.config.driftSpeedDiffLimit.x);
                float positiveModifier = 1 - (speedDiff / car.config.driftSpeedDiffLimit.y);
                car.inputData.drift.y += (speedDot > 0 ? positiveModifier : negativeModifier) * speedDot;
            }

            if (car.inputData.accelerate < MathHelper.epsilon) ApplyBrakeForce(0.005f, wheelData);
        }
        else
        {
            grounded = false;

            springLength = car.config.springMaxTravel;
        }

        if (grounded && ((car.inputData.accelerate > 0.5f && wheelData.gripFactor < smokeThreshold) || car.inputData.drift.x > 0))
        {
            if (!isSmokePlaying)
            {
                driftSmoke.Play();
                isSmokePlaying = true;
            }
        }
        if (!grounded || ((car.inputData.accelerate < 0.1f || wheelData.gripFactor > smokeThreshold) && car.inputData.drift.x <= 0))
        {
            if (isSmokePlaying)
            {
                driftSmoke.Stop();
                isSmokePlaying = false;
            }
        }
    }

    private void Update()
    {
        wheelVisual.transform.position = Vector3.MoveTowards(wheelVisual.transform.position, WheelPosition, 1f * Time.deltaTime);
        wheelVisual.transform.rotation = Quaternion.RotateTowards(wheelVisual.transform.rotation, transform.rotation, 90 * Time.deltaTime);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (Application.isPlaying) Gizmos.DrawLine(transform.position, WheelPosition);
        else Gizmos.DrawLine(transform.position, transform.position + (car.config.springRestDistance * -transform.up));

        WheelData wheelData = CalculateWheelData();
        Gizmos.color = Color.Lerp(Color.red, Color.green, wheelData.gripFactor);

        if (Application.isPlaying) Gizmos.DrawSphere(WheelPosition, car.config.wheelRadius);
        else Gizmos.DrawSphere(transform.position + (car.config.springRestDistance * -transform.up), car.config.wheelRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (car.config.springRestDistance * -transform.up));
    }
#endif
}
