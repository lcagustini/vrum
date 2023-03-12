using SceneRefAttributes;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviourValidated
{
    [SerializeField, Parent] private CarController car;
    [SerializeField, Child(Flag.Optional)] private ParticleSystem driftSmoke;

    public bool motorWheel;

    public float wheelRadius;
    public float wheelTurnDegrees;
    public float wheelMass;
    [SerializeField, Anywhere] public Transform wheelVisual;

    public float springMaxTravel;
    public float springRestDistance;
    [ReadOnly] private float springLength;
    public float springStrength;
    public float dampingStrength;

    public AnimationCurve gripFactorCurve;

    public Vector3 WheelPosition => transform.position + (springLength * -transform.up);

    private void ApplySuspensionForce()
    {
        Vector3 velocity = car.RB.GetPointVelocity(transform.position);

        float upComponent = Vector3.Dot(transform.up, velocity);

        float springForce = springStrength * (springRestDistance - springLength);
        float dampingForce = dampingStrength * upComponent;
        Vector3 forceVector = (springForce - dampingForce) * transform.up;

        Debug.DrawLine(transform.position, transform.position + forceVector, Color.magenta);
        car.RB.AddForceAtPosition(forceVector, transform.position);
    }

    private void ApplySteeringForce()
    {
        Vector3 velocity = car.RB.GetPointVelocity(transform.position);
        float speed = velocity.magnitude;

        float sidewaysComponent = Vector3.Dot(transform.right, velocity);
        float sidewaysRatio = Mathf.Abs(sidewaysComponent / speed);

        float gripFactor = gripFactorCurve.Evaluate(sidewaysRatio);
        float gripForce = -gripFactor * sidewaysComponent * wheelMass / Time.fixedDeltaTime;
        Vector3 forceVector = gripForce * transform.right;

        Debug.DrawLine(transform.position, transform.position + forceVector, Color.magenta);
        car.RB.AddForceAtPosition(forceVector, transform.position);
    }

    private void ApplyAccelerationForce()
    {
        if (motorWheel)
        {
            Vector3 velocity = car.RB.GetPointVelocity(transform.position);

            float forwardComponent = Vector3.Dot(transform.forward, velocity);
            float forwardRatio = forwardComponent / car.topSpeed;

            float motorTorque = car.inputData.accelerate * car.motorMaxTorque * car.motorTorqueResponseCurve.Evaluate(forwardRatio);
            Vector3 forceVector = transform.forward * motorTorque;

            Debug.DrawLine(transform.position, transform.position + forceVector, Color.magenta);
            car.RB.AddForceAtPosition(forceVector, transform.position);
        }
    }

    private void ApplyBrakeForce(float input)
    {
        Vector3 velocity = car.RB.GetPointVelocity(transform.position);

        float forwardComponent = Vector3.Dot(transform.forward, velocity);
        float forwardRatio = Mathf.Abs(forwardComponent / car.topSpeed);

        float brakeFactor = input * car.brakeResponseCurve.Evaluate(forwardRatio);
        float brakeForce = -brakeFactor * forwardComponent * wheelMass / Time.fixedDeltaTime;
        Vector3 forceVector = brakeForce * transform.forward;

        Debug.DrawLine(transform.position, transform.position + forceVector, Color.magenta);
        car.RB.AddForceAtPosition(forceVector, transform.position);
    }

    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hitInfo, springMaxTravel + wheelRadius))
        {
            springLength = hitInfo.distance - wheelRadius;

            ApplySuspensionForce();
            ApplySteeringForce();
            ApplyAccelerationForce();
            ApplyBrakeForce(car.inputData.brake);

            if (car.inputData.accelerate < 0.0001f) ApplyBrakeForce(0.01f);
        }
        else
        {
            springLength = springMaxTravel;
        }
    }

    private void Update()
    {
        transform.rotation = car.transform.rotation * Quaternion.AngleAxis(wheelTurnDegrees * car.inputData.steer, car.transform.up);

        wheelVisual.transform.position = WheelPosition;
        wheelVisual.transform.rotation = transform.rotation;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (Application.isPlaying) Gizmos.DrawLine(transform.position, WheelPosition);
        else Gizmos.DrawLine(transform.position, transform.position + (springRestDistance * -transform.up));

        Gizmos.color = Color.black;
        if (Application.isPlaying) Gizmos.DrawWireSphere(WheelPosition, wheelRadius);
        else Gizmos.DrawWireSphere(transform.position + (springRestDistance * -transform.up), wheelRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (springRestDistance * -transform.up));
    }
}
