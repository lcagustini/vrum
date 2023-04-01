using SceneRefAttributes;
using Unity.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class Wheel : MonoBehaviourValidated
{
    private struct WheelData
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

    [SerializeField, Anywhere] public Transform wheelVisual;

    [SerializeField] private WheelType wheelType;

    [SerializeField] private float smokeThreshold;

    private float springLength;
    private bool grounded;

    public Vector3 WheelPosition => transform.position + (springLength * -transform.up);

    private bool IsMotorWheel => ((wheelType == WheelType.BackLeft || wheelType == WheelType.BackRight) && (car.config.drivetrain == Drivetrain.RearWheelDrive || car.config.drivetrain == Drivetrain.AllWheelDrive)) || ((wheelType == WheelType.FrontLeft || wheelType == WheelType.FrontRight) && (car.config.drivetrain == Drivetrain.FrontWheelDrive || car.config.drivetrain == Drivetrain.AllWheelDrive));
    private bool IsSteeringWheel => wheelType == WheelType.FrontLeft || wheelType == WheelType.FrontRight;

    private void ApplySuspensionForce(WheelData wheelData)
    {
        float springForce = car.config.springStrength * (car.config.springRestDistance - springLength);
        float dampingForce = car.config.dampingStrength * wheelData.upComponent;
        Vector3 forceVector = (springForce - dampingForce) * transform.up;

        Debug.DrawLine(transform.position, transform.position + (forceVector / car.RB.mass), Color.magenta);
        car.RB.AddForceAtPosition(forceVector, transform.position);
    }

    private void ApplySteeringForce(WheelData wheelData)
    {
        float gripForce = -wheelData.gripFactor * wheelData.sidewaysComponent * car.config.wheelMass / Time.fixedDeltaTime;

        Vector3 forceVector = gripForce * transform.right;

        Debug.DrawLine(transform.position, transform.position + (forceVector / car.RB.mass), Color.magenta);
        car.RB.AddForceAtPosition(forceVector, transform.position);
    }

    private void ApplyAccelerationForce(WheelData wheelData)
    {
        if (IsMotorWheel)
        {
            float motorTorqueFactor = Mathf.Clamp(car.inputData.accelerate * car.config.steeringAccelerationFactorCurves[(int)wheelType].Evaluate(wheelData.sidewaysRatio) * car.config.motorTorqueResponseCurve[car.inputData.gear].Evaluate(wheelData.forwardRatio), -1.0f, 1.0f);
            float motorTorque = motorTorqueFactor * car.config.motorMaxTorque;

            Vector3 forceVector = wheelData.gripFactor * motorTorque * transform.forward;

            Debug.DrawLine(transform.position, transform.position + (forceVector / car.RB.mass), Color.magenta);
            car.RB.AddForceAtPosition(forceVector, transform.position);
        }
    }

    private void ApplyBrakeForce(float input, WheelData wheelData)
    {
        float brakeFactor = input * car.config.brakeResponseCurve.Evaluate(wheelData.forwardRatio);
        float brakeForce = -brakeFactor * wheelData.forwardComponent * car.config.wheelMass / Time.fixedDeltaTime;

        Vector3 forceVector = brakeForce * wheelData.gripFactor * transform.forward;

        Debug.DrawLine(transform.position, transform.position + (forceVector / car.RB.mass), Color.magenta);
        car.RB.AddForceAtPosition(forceVector, transform.position);
    }

    private WheelData CalculateWheelData()
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

        wheelData.gripFactor = car.config.speedGripFactorCurves[(int)wheelType].Evaluate(wheelData.speedRatio) * car.config.sidewaysGripFactorCurves[(int)wheelType].Evaluate(wheelData.sidewaysRatio);

        return wheelData;
    }

    private void FixedUpdate()
    {
        WheelData wheelData = CalculateWheelData();

        if (wheelData.gripFactor < 0.6f)
        {
            driftSmoke.SetVector3("Color", new Vector3(1, 0, 0));
        }
        else
        {
            driftSmoke.SetVector3("Color", new Vector3(1, 1, 1));
        }

        if (IsSteeringWheel)
        {
            float turnAmount = (car.config.wheelForwardTurnModifier.Evaluate(wheelData.forwardRatio) + car.config.wheelSidewaysTurnModifier.Evaluate(wheelData.sidewaysRatio)) * car.config.wheelMaxTurnDegrees;

            transform.localRotation = Quaternion.Euler(0, turnAmount * car.inputData.steer, 0);
        }

        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hitInfo, car.config.springMaxTravel + car.config.wheelRadius))
        {
            grounded = true;

            springLength = hitInfo.distance - car.config.wheelRadius;

            ApplySuspensionForce(wheelData);
            ApplySteeringForce(wheelData);
            ApplyAccelerationForce(wheelData);
            ApplyBrakeForce(car.inputData.brake, wheelData);

            if (car.inputData.accelerate < MathHelper.epsilon) ApplyBrakeForce(0.005f, wheelData);
        }
        else
        {
            grounded = false;

            springLength = car.config.springMaxTravel;
        }

        if (grounded && car.inputData.accelerate > 0.5f && wheelData.gripFactor < smokeThreshold && !isSmokePlaying)
        {
            driftSmoke.Play();
            isSmokePlaying = true;
        }
        if ((!grounded || car.inputData.accelerate < 0.1f || wheelData.gripFactor > smokeThreshold) && isSmokePlaying)
        {
            driftSmoke.Stop();
            isSmokePlaying = false;
        }
    }

    private void Update()
    {
        wheelVisual.transform.position = Vector3.MoveTowards(wheelVisual.transform.position, WheelPosition, 0.1f * Time.deltaTime);
        wheelVisual.transform.rotation = Quaternion.RotateTowards(wheelVisual.transform.rotation, transform.rotation, 10 * Time.deltaTime);
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
