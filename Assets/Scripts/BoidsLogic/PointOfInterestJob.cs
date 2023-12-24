using Models;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace BoidsLogic
{
    public struct PointOfInterestJob : IJobParallelFor
    {
        private NativeArray<Vector3> _positions;
        private NativeArray<Vector3> _accelerations;
        private AccelerationWeights _weights;
        private Vector3 _interestPosition;

        private const float ConsumeRadius = 0.5f;

        [NativeDisableParallelForRestriction] private NativeArray<PointOfInterest> _pointsOfInterest;

        public PointOfInterestJob(NativeArray<Vector3> positions, NativeArray<Vector3> accelerations, 
            AccelerationWeights weights, Vector3 interestPosition, NativeArray<PointOfInterest> pointsOfInterest)
        {
            _positions = positions;
            _accelerations = accelerations;
            _weights = weights;
            _interestPosition = interestPosition;
            _pointsOfInterest = pointsOfInterest;
        }

        public void Execute(int index)
        {
            /*Vector3 acceleration = _interestPosition - _positions[index];
            _accelerations[index] += _weights.PointOfInterest * acceleration;*/

            if (_pointsOfInterest.Length == 0)
            {
                Debug.Log("Zero");
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
                Debug.Log("Consumed");
            }

            Vector3 acceleration = _pointsOfInterest[chosenPointIndex].Position - _positions[index];
            _accelerations[index] += _weights.PointOfInterest * acceleration;
            
        }
        
    }
}