using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Drivetrain
{
    RearWheelDrive,
    FrontWheelDrive,
    AllWheelDrive
}

public enum WheelType
{
    FrontLeft,
    FrontRight,
    BackLeft,
    BackRight
}

[CreateAssetMenu(fileName = "NewCarConfig", menuName = "Race/Create Car Config")]
public class CarConfig : ScriptableObject
{
    public float carMass;
    public Vector3 centerOfMass;

    public float topSpeed;
    public Drivetrain drivetrain;

    public Vector2 automaticGearLimits;
    public List<AnimationCurve> motorTorqueResponseCurve;
    public float motorMaxTorque;

    public AnimationCurve brakeResponseCurve;
    public List<AnimationCurve> brakeGripLossCurves;

    public float wheelRadius;
    public float wheelMass;

    public AnimationCurve wheelSidewaysTurnModifier;
    public AnimationCurve wheelForwardTurnModifier;
    public float wheelMaxTurnDegrees;

    public float springMaxTravel;
    public float springRestDistance;
    public float springStrength;
    public float dampingStrength;

    public List<AnimationCurve> steeringAccelerationFactorCurves;
    public float driftAdjustSpeed;
    public float driftSteerLimit;
    public float driftCarAngleModifier;
    public float gripToDriftThreshold;
    public float smokeThreshold;
    public AnimationCurve driftRotationScaling;

    public List<AnimationCurve> sidewaysGripFactorCurves;
    public List<AnimationCurve> speedGripFactorCurves;
}
