using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarMLController : Agent, ICarController
{
    private float followParameter;
    private float checkpoint;

    public GameObject GameObject => gameObject;
    public Car Car { get; set; }

    public override void OnEpisodeBegin()
    {
        Car.RB.angularVelocity = Vector3.zero;
        Car.RB.velocity = Vector3.zero;

        Car.RB.position = Car.gridPoint.transform.position;
        Car.RB.rotation = Car.gridPoint.transform.rotation;

        LapManager.Instance.checkpointTracker[Car] = new LapManager.LapTracker(Time.timeSinceLevelLoad, 1, 0);
        checkpoint = 0;
        followParameter = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 followPoint = LapManager.Instance.racingLine.EvaluatePosition(followParameter);
        Vector3 followDir = followPoint - Car.transform.position;
        float sideToFollow = Mathf.Abs(Vector3.Dot(Car.transform.right, followDir.normalized));
        float cosToFollow = Vector3.Dot(Car.transform.right, followDir);

        sensor.AddObservation(Car.inputData.drift);
        sensor.AddObservation(Car.RB.velocity);
        sensor.AddObservation(sideToFollow);
        sensor.AddObservation(cosToFollow);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Car.inputData.accelerate = Mathf.Clamp01(actions.ContinuousActions[0]);
        Car.inputData.brake = Mathf.Clamp01(actions.ContinuousActions[1]);
        Car.inputData.steer = Mathf.Clamp(actions.ContinuousActions[2], -1, 1);

        Vector3 followPoint = LapManager.Instance.racingLine.EvaluatePosition(followParameter);
        Vector3 followDir = followPoint - Car.transform.position;
        float followDot = Vector3.Dot(followDir, Car.transform.forward);

        AddReward(Car.RB.velocity.magnitude * 0.005f);

        if (followDot < -5)
        {
            AddReward(-0.1f);
        }

        if (Car.wheels.Any(w => w.groundType == Wheel.GroundType.Dirt))
        {
            AddReward(-0.01f);
        }

        if (LapManager.Instance.checkpointTracker[Car].checkpoint > checkpoint)
        {
            AddReward(10.0f);
            checkpoint = LapManager.Instance.checkpointTracker[Car].checkpoint;
        }

        if (LapManager.Instance.checkpointTracker[Car].lap > 1)
        {
            AddReward(100.0f);
            EndEpisode();
        }

        if (LapManager.Instance.GetRunningTime(Car) > 60.0f)
        {
            EndEpisode();
        }
    }

    private void Update()
    {
        Vector3 followPoint = LapManager.Instance.racingLine.EvaluatePosition(followParameter);
        Vector3 followDir = followPoint - Car.transform.position;
        float followDot = Vector3.Dot(followDir, Car.transform.forward);

        Debug.DrawLine(Car.transform.position, followPoint, Color.cyan);

        float checkDistance = (1 + Car.RB.velocity.magnitude / Car.config.topSpeed) * (followDot > 0 ? 30 : 60);
        if (followDir.magnitude < checkDistance)
        {
            followParameter += 0.004f;
            if (followParameter > 1) followParameter -= 1;
        }
    }
}
