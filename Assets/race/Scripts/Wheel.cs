using SceneRefAttributes;
using Unity.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wheel : MonoBehaviourValidated
{
    [SerializeField, Parent] private CarController car;
    [SerializeField, Child(Flag.Optional)] private ParticleSystem driftSmoke;
    [SerializeField, Anywhere] public Transform wheelVisual;

    [SerializeField] private WheelType wheelType;

    private float springLength;

    public Vector3 WheelPosition => transform.position + (springLength * -transform.up);

    private bool IsMotorWheel => ((wheelType == WheelType.BackLeft || wheelType == WheelType.BackRight) && (car.config.drivetrain == Drivetrain.RearWheelDrive || car.config.drivetrain == Drivetrain.AllWheelDrive)) || ((wheelType == WheelType.FrontLeft || wheelType == WheelType.FrontRight) && (car.config.drivetrain == Drivetrain.FrontWheelDrive || car.config.drivetrain == Drivetrain.AllWheelDrive));
    private bool IsSteeringWheel => wheelType == WheelType.FrontLeft || wheelType == WheelType.FrontRight;

    private void ApplySuspensionForce()
    {
        Vector3 velocity = car.RB.GetPointVelocity(transform.position);

        float upComponent = Vector3.Dot(transform.up, velocity);

        float springForce = car.config.springStrength * (car.config.springRestDistance - springLength);
        float dampingForce = car.config.dampingStrength * upComponent;
        Vector3 forceVector = (springForce - dampingForce) * transform.up;

        Debug.DrawLine(transform.position, transform.position + (forceVector / car.RB.mass), Color.magenta);
        car.RB.AddForceAtPosition(forceVector, transform.position);
    }

    private void ApplySteeringForce()
    {
        Vector3 velocity = car.RB.GetPointVelocity(transform.position);
        float speed = velocity.magnitude;
        float speedRatio = speed / car.config.topSpeed;

        float sidewaysComponent = Vector3.Dot(transform.right, velocity);
        float sidewaysRatio = Mathf.Abs(sidewaysComponent / speed);

        float gripFactor = car.config.speedGripFactorCurves[(int)wheelType].Evaluate(speedRatio) * car.config.sidewaysGripFactorCurves[(int)wheelType].Evaluate(sidewaysRatio);
        float gripForce = -gripFactor * sidewaysComponent * car.config.wheelMass / Time.fixedDeltaTime;
        Vector3 forceVector = gripForce * transform.right;

        Debug.DrawLine(transform.position, transform.position + (forceVector / car.RB.mass), Color.magenta);
        car.RB.AddForceAtPosition(forceVector, transform.position);
    }

    private void ApplyAccelerationForce()
    {
        if (IsMotorWheel)
        {
            Vector3 velocity = car.RB.GetPointVelocity(transform.position);

            float forwardComponent = Vector3.Dot(transform.forward, velocity);
            float forwardRatio = forwardComponent / car.config.topSpeed;

            float motorTorque = car.inputData.accelerate * car.config.motorMaxTorque * car.config.motorTorqueResponseCurve.Evaluate(forwardRatio);
            Vector3 forceVector = transform.forward * motorTorque;

            Debug.DrawLine(transform.position, transform.position + (forceVector / car.RB.mass), Color.magenta);
            car.RB.AddForceAtPosition(forceVector, transform.position);
        }
    }

    private void ApplyBrakeForce(float input)
    {
        Vector3 velocity = car.RB.GetPointVelocity(transform.position);

        float forwardComponent = Vector3.Dot(transform.forward, velocity);
        float forwardRatio = Mathf.Abs(forwardComponent / car.config.topSpeed);

        float brakeFactor = input * car.config.brakeResponseCurve.Evaluate(forwardRatio);
        float brakeForce = -brakeFactor * forwardComponent * car.config.wheelMass / Time.fixedDeltaTime;
        Vector3 forceVector = brakeForce * transform.forward;

        Debug.DrawLine(transform.position, transform.position + (forceVector / car.RB.mass), Color.magenta);
        car.RB.AddForceAtPosition(forceVector, transform.position);
    }

    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hitInfo, car.config.springMaxTravel + car.config.wheelRadius))
        {
            springLength = hitInfo.distance - car.config.wheelRadius;

            ApplySuspensionForce();
            ApplySteeringForce();
            ApplyAccelerationForce();
            ApplyBrakeForce(car.inputData.brake);

            if (car.inputData.accelerate < MathHelper.epsilon) ApplyBrakeForce(0.005f);
        }
        else
        {
            springLength = car.config.springMaxTravel;
        }
    }

    private void Update()
    {
        if (IsSteeringWheel)
        {
            transform.rotation = car.transform.rotation * Quaternion.AngleAxis((car.inputData.powerTurn ? car.config.wheelPowerTurnDegrees : car.config.wheelNormalTurnDegrees) * car.inputData.steer, car.transform.up);
        }

        wheelVisual.transform.position = WheelPosition;
        wheelVisual.transform.rotation = transform.rotation;

        if (car.inputData.powerTurn && !driftSmoke.isPlaying) driftSmoke.Play();
        if (!car.inputData.powerTurn && driftSmoke.isPlaying) driftSmoke.Stop();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (Application.isPlaying) Gizmos.DrawLine(transform.position, WheelPosition);
        else Gizmos.DrawLine(transform.position, transform.position + (car.config.springRestDistance * -transform.up));

        {
            Vector3 velocity = car.RB.GetPointVelocity(transform.position);
            float speed = velocity.magnitude;
            float speedRatio = speed / car.config.topSpeed;

            float sidewaysComponent = Vector3.Dot(transform.right, velocity);
            float sidewaysRatio = Mathf.Abs(sidewaysComponent / speed);

            float gripFactor = car.config.speedGripFactorCurves[(int)wheelType].Evaluate(speedRatio) * car.config.sidewaysGripFactorCurves[(int)wheelType].Evaluate(sidewaysRatio);

            Gizmos.color = Color.Lerp(Color.red, Color.green, gripFactor);
        }

        if (Application.isPlaying) Gizmos.DrawSphere(WheelPosition, car.config.wheelRadius);
        else Gizmos.DrawSphere(transform.position + (car.config.springRestDistance * -transform.up), car.config.wheelRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (car.config.springRestDistance * -transform.up));
    }
#endif
}
