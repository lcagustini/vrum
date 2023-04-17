using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAIController : CarController
{
    [SerializeField] private AnimationCurve steerCurve;

    private float followParameter;

    private void Update()
    {
        if (car.inputData.gear == 0) car.inputData.gear = 1;

        bool drifting = car.inputData.drift > 0;

        Vector3 followPoint = LapManager.Instance.racingLine.EvaluatePosition(followParameter);
        Vector3 followDir = followPoint - car.transform.position;
        float sideToFollow = Vector3.Dot(car.transform.right, followDir.normalized);

        float brakeParameter = followParameter + 0.015f;
        if (brakeParameter > 1) brakeParameter -= 1;
        Vector3 brakePoint = LapManager.Instance.racingLine.EvaluatePosition(brakeParameter);
        Vector3 brakeDir = brakePoint - car.transform.position;
        float sideToBrake = Vector3.Dot(car.transform.right, brakeDir.normalized);

        float directionMultiplier = (Vector3.Dot(followDir, car.transform.right) * Vector3.Dot(brakeDir, car.transform.right) < 0) ? 0 : 1;
        float driftMultiplier = drifting ? 0 : 1;

        car.inputData.accelerate = driftMultiplier * directionMultiplier * Mathf.Clamp01(3 * (car.config.topSpeed - car.RB.velocity.magnitude) / car.config.topSpeed);
        car.inputData.steer = Mathf.Clamp(3f * sideToFollow, -1, 1);
        car.inputData.brake = driftMultiplier * (Mathf.Clamp01(3 * Mathf.Abs(sideToBrake) - 1) + Mathf.Clamp01(4 * Mathf.Abs(sideToFollow) - 1)) / 2;

        float checkDistance = (1 + car.RB.velocity.magnitude / car.config.topSpeed) * (drifting ? 40 : 30);
        if (followDir.magnitude < checkDistance)
        {
            followParameter += 0.004f;
            if (followParameter > 1) followParameter -= 1;
        }

        Debug.DrawLine(car.transform.position, followPoint, Color.cyan);
        Debug.DrawLine(car.transform.position, brakePoint, Color.yellow);
    }
}
