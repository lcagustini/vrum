using SceneRefAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class CarController : MonoBehaviourValidated
{
    public struct InputData
    {
        public float steer;
        public float accelerate;
        public float brake;
    }

    [SerializeField, Self] public Rigidbody RB;

    public float topSpeed;

    public AnimationCurve motorTorqueResponseCurve;
    public float motorMaxTorque;

    public AnimationCurve brakeResponseCurve;

    public InputData inputData;

    public void Steer(CallbackContext context)
    {
        inputData.steer = context.ReadValue<float>();
    }

    public void Accelerate(CallbackContext context)
    {
        inputData.accelerate = context.ReadValue<float>();
    }

    public void Brake(CallbackContext context)
    {
        inputData.brake = context.ReadValue<float>();
    }
}

[CustomEditor(typeof(CarController))]
public class CarControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CarController car = (CarController)target;

        base.OnInspectorGUI();

        if (GUILayout.Button("Add force"))
        {
            car.RB.AddForce(2000 * car.transform.forward);
        }
    }
}