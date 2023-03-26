using SceneRefAttributes;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class LapManager : SingletonMonoBehaviourValidated<LapManager>
{
    public class LapTracker
    {
        public float lapStartTime;
        public int lap;
        public int checkpoint;

        public LapTracker(float lapStartTime, int lap, int checkpoint)
        {
            this.lapStartTime = lapStartTime;
            this.lap = lap;
            this.checkpoint = checkpoint;
        }
    }

    [SerializeField, Child] private CheckpointCollider[] checkpoints;
    public Dictionary<CarController, LapTracker> checkpointTracker = new Dictionary<CarController, LapTracker>();

    private void Start()
    {
        CheckpointCollider checkpoint = checkpoints.First(c => c.order == 0);
    }

    public void UpdateCheckpoint(CarController car, int order)
    {
        if (order == checkpointTracker[car].checkpoint + 1)
        {
            checkpointTracker[car].checkpoint = order;
        }

        if (order == 0 && checkpointTracker[car].checkpoint == checkpoints.Max(c => c.order))
        {
            Debug.Log($"Lap {checkpointTracker[car].lap}: {Time.timeSinceLevelLoad - checkpointTracker[car].lapStartTime}");
            checkpointTracker[car].checkpoint = 0;
            checkpointTracker[car].lap++;
            checkpointTracker[car].lapStartTime = Time.timeSinceLevelLoad;
        }
    }
}
