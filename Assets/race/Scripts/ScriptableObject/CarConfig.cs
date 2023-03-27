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
    public float wheelNormalTurnDegrees;
    public float wheelPowerTurnDegrees;
    public float wheelMass;

    public float springMaxTravel;
    public float springRestDistance;
    public float springStrength;
    public float dampingStrength;

    public List<AnimationCurve> accelerationSteeringFactorCurves;

    public List<AnimationCurve> sidewaysGripFactorCurves;
    public List<AnimationCurve> speedGripFactorCurves;
    public float gripMultiplier;
}
