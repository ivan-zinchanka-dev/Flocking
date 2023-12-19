using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    public struct DetectNearbyEntitiesJob : IJobParallelFor
    {
        public float DetectionRadius { get; private set; }

        public DetectNearbyEntitiesJob(float detectionRadius)
        {
            DetectionRadius = detectionRadius;
            
            Debug.Log("Rad: " + DetectionRadius);
        }

        public void Execute(int index)
        {
            Debug.Log("Detect");
        }
    }
}