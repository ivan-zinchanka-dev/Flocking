using Models;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    public struct PointOfInterestJob : IJobParallelFor
    {
        private NativeArray<Vector3> _positions;
        private NativeArray<Vector3> _accelerations;

        private const float ConsumeRadius = 1.0f;
        private const float Weight = 0.1f;

        [NativeDisableParallelForRestriction] private NativeArray<PointOfInterest> _pointsOfInterest;

        public PointOfInterestJob(NativeArray<Vector3> positions, NativeArray<Vector3> accelerations, 
            NativeArray<PointOfInterest> pointsOfInterest)
        {
            _positions = positions;
            _accelerations = accelerations;
            _pointsOfInterest = pointsOfInterest;
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

            if (Vector3.Distance(_positions[index], _pointsOfInterest[chosenPointIndex].Position) < ConsumeRadius)
            {
                PointOfInterest cachedPoint = _pointsOfInterest[chosenPointIndex];
                _pointsOfInterest[chosenPointIndex] = new PointOfInterest(cachedPoint.Id, cachedPoint.Position, true);
            }

            Vector3 acceleration = _pointsOfInterest[chosenPointIndex].Position - _positions[index];
            _accelerations[index] += Weight * acceleration;
            
        }
    }
}