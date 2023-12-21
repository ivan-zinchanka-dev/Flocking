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

        public PointOfInterestJob(NativeArray<Vector3> positions, NativeArray<Vector3> accelerations, 
            AccelerationWeights weights, Vector3 interestPosition)
        {
            _positions = positions;
            _accelerations = accelerations;
            _weights = weights;
            _interestPosition = interestPosition;
        }

        public void Execute(int index)
        {
            Vector3 acceleration = _interestPosition - _positions[index];
            _accelerations[index] += _weights.PointOfInterest * acceleration;
            
            /*if (!_consumed[0] && Vector3.Distance(_interestPosition, _positions[index]) <= ConsumeRadius)
            {
                Debug.Log("Consume internal");
                _consumed[0] = true;
            }*/
        }
        
    }
}