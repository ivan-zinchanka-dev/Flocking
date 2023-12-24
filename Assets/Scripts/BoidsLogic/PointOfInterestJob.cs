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

        private NativeArray<Vector3> _pointsOfInterest;

        public PointOfInterestJob(NativeArray<Vector3> positions, NativeArray<Vector3> accelerations, 
            AccelerationWeights weights, Vector3 interestPosition, NativeArray<Vector3> pointsOfInterest)
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

            for (int i = 0; i < _pointsOfInterest.Length; i++)
            {
                
            }
            
            
        }
        
    }
}