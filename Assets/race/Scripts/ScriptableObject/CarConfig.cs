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
    public float topSpeed;
    public Drivetrain drivetrain;

    public List<AnimationCurve> motorTorqueResponseCurve;
    public float motorMaxTorque;

    public AnimationCurve brakeResponseCurve;

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

    public List<AnimationCurve> sidewaysGripFactorCurves;
    public List<AnimationCurve> speedGripFactorCurves;
}
