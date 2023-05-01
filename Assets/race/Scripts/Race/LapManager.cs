using KBCore.Refs;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class LapManager : SingletonMonoBehaviourValidated<LapManager>
{
    public class LapTracker
    {
        public float currentLapStartTime;
        public int lap;
        public int checkpoint;

        public List<float> lapTimes;

        public LapTracker(float lapStartTime, int lap, int checkpoint)
        {
            this.currentLapStartTime = lapStartTime;
            this.lap = lap;
            this.checkpoint = checkpoint;

            lapTimes = new List<float>();
        }
    }

    [SerializeField, Child] private CheckpointCollider[] checkpoints;
    private int availableGridPoint;

    [SerializeField, Child] private StartingGridPoint[] gridPoints;
    public Dictionary<Car, LapTracker> checkpointTracker = new Dictionary<Car, LapTracker>();

    [SerializeField, Child] public SplineContainer racingLine;

    [SerializeField] public int totalLaps;

    public bool HasGridPointAvailable => availableGridPoint < gridPoints.Length;

    private void Start()
    {
        CheckpointCollider checkpoint = checkpoints.First(c => c.order == 0);
    }

    public StartingGridPoint AllocateGridPoint()
    {
        if (availableGridPoint >= gridPoints.Length) return null;

        StartingGridPoint point = gridPoints[availableGridPoint];
        availableGridPoint++;
        return point;
    }

    public void UpdateCheckpoint(Car car, int order)
    {
        if (checkpointTracker[car].lap == totalLaps + 1) return;

        if (order == checkpointTracker[car].checkpoint + 1)
        {
            checkpointTracker[car].checkpoint = order;
        }

        if (order == 0 && checkpointTracker[car].checkpoint == checkpoints.Max(c => c.order))
        {
            checkpointTracker[car].lapTimes.Add(Time.timeSinceLevelLoad - checkpointTracker[car].currentLapStartTime);
            checkpointTracker[car].checkpoint = 0;
            checkpointTracker[car].lap++;
            checkpointTracker[car].currentLapStartTime = Time.timeSinceLevelLoad;

            Debug.Log($"Lap {checkpointTracker[car].lapTimes.Count}: {checkpointTracker[car].lapTimes[checkpointTracker[car].lapTimes.Count - 1]}");
        }

        if (checkpointTracker[car].lap == totalLaps + 1)
        {
            RaceManager.Instance.firstRacingCar++;

            if (car.controller is CarPlayerController)
            {
                RaceManager.Instance.ConvertPlayerCarToAI(car);
            }

            Debug.Log("Finished race");
        }
    }

    public int GetLap(Car car)
    {
        return checkpointTracker[car].lap;
    }

    public CheckpointCollider GetCheckpoint(Car car)
    {
        return checkpoints.FirstOrDefault(c => c.order == checkpointTracker[car].checkpoint);
    }

    public CheckpointCollider GetNextCheckpoint(Car car)
    {
        int maxOrder = checkpoints.Max(c => c.order);
        int nextOrder = checkpointTracker[car].checkpoint >= maxOrder ? checkpointTracker[car].checkpoint - maxOrder : checkpointTracker[car].checkpoint + 1;
        return checkpoints.FirstOrDefault(c => c.order == nextOrder);
    }

    public float GetRunningTime(Car car)
    {
        if (!checkpointTracker.ContainsKey(car)) return 0;
        return Time.timeSinceLevelLoad - checkpointTracker[car].currentLapStartTime;
    }

    public float GetBestTime(Car car)
    {
        if (!checkpointTracker.ContainsKey(car) || checkpointTracker[car].lapTimes.Count == 0) return 0;
        return checkpointTracker[car].lapTimes.Min();
    }
}
