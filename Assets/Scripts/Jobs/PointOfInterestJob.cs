using Models;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    [BurstCompile]
    public struct PointOfInterestJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] private NativeArray<PointOfInterest> _pointsOfInterest;
        
        [ReadOnly] private NativeArray<Vector3> _positions;
        private NativeArray<Vector3> _accelerations;

        private readonly float _consumeRadius;
        private readonly float _accelerationWeight;
        
        public PointOfInterestJob(NativeArray<PointOfInterest> pointsOfInterest, NativeArray<Vector3> positions, NativeArray<Vector3> accelerations, float consumeRadius, float accelerationWeight)
        {
            _pointsOfInterest = pointsOfInterest;
            _positions = positions;
            _accelerations = accelerations;
            _consumeRadius = consumeRadius;
            _accelerationWeight = accelerationWeight;
        }

        public void Execute(int index)
        {
            if (_pointsOfInterest.Length == 0)
            {
                return;
            }
            
            float minDistance = float.MaxValue;
            int chosenPointIndex = 0;
            
            for (int i = 0; i < _pointsOfInterest.Length; i++)
            {
                float distanceToPoint = Vector3.Distance(_positions[index], _pointsOfInterest[i].Position);
                
                if (distanceToPoint < minDistance)
                {
                    minDistance = distanceToPoint;
                    chosenPointIndex = i;
                }
            }

            if (Vector3.Distance(_positions[index], _pointsOfInterest[chosenPointIndex].Position) < _consumeRadius)
            {
                PointOfInterest cachedPoint = _pointsOfInterest[chosenPointIndex];
                _pointsOfInterest[chosenPointIndex] = new PointOfInterest(cachedPoint.Id, cachedPoint.Position, 1);
            }

            Vector3 acceleration = _pointsOfInterest[chosenPointIndex].Position - _positions[index];
            _accelerations[index] += _accelerationWeight * acceleration;
            
        }
    }
}