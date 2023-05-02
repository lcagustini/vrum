using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KBCore.Refs;
using Cinemachine;

public class CarAIController : MonoBehaviour, ICarController
{
    private float followParameter;

    public CinemachineVirtualCamera VirtualCamera => null;
    public GameObject GameObject => gameObject;
    public Car Car { get; set; }

    public float Sigmoid(float value)
    {
        Debug.Log($"{Car} {value}");
        value = 5 * value;
        float k = Mathf.Exp(value);
        return k / (1.0f + k);
    }

    private void Update()
    {
        bool drifting = Car.inputData.drift > 0;

        Vector3 followPoint = LapManager.Instance.racingLine.EvaluatePosition(followParameter);
        Vector3 followDir = followPoint - Car.transform.position;
        float sideToFollow = Vector3.Dot(Car.transform.right, followDir.normalized);

        float brakeParameter = followParameter + 0.02f;
        if (brakeParameter > 1) brakeParameter -= 1;
        Vector3 brakePoint = LapManager.Instance.racingLine.EvaluatePosition(brakeParameter);
        Vector3 brakeDir = brakePoint - Car.transform.position;
        float sideToBrake = Vector3.Dot(Car.transform.right, brakeDir.normalized);

        float driftMultiplier = drifting ? 0 : 1;

        Car.inputData.accelerate = driftMultiplier * Mathf.Clamp01(3 * (Car.config.TopSpeed(Car) - Car.RB.velocity.magnitude) / Car.config.TopSpeed(Car));
        Car.inputData.steer = Mathf.Clamp(3f * sideToFollow, -1, 1);
        Car.inputData.brake = driftMultiplier * (Mathf.Clamp01(3 * Mathf.Abs(sideToBrake) - 1) + Mathf.Clamp01(4 * Mathf.Abs(sideToFollow) - 1)) / 2;
        //Car.inputData.brake = driftMultiplier * Sigmoid(-75 * Vector3.Dot((followPoint - Car.transform.position).normalized, (brakePoint - Car.transform.position).normalized) + 74);

        if (Car.inputData.brake > 0.8f) Car.inputData.accelerate = 0;

        float checkDistance = (1 + Car.RB.velocity.magnitude / Car.config.TopSpeed(Car)) * (drifting ? 40 : 30);
        if (followDir.magnitude < checkDistance)
        {
            followParameter += 0.004f;
            if (followParameter > 1) followParameter -= 1;
        }

        Debug.DrawLine(Car.transform.position, followPoint, Color.cyan);
        Debug.DrawLine(Car.transform.position, brakePoint, Color.yellow);
    }
}
