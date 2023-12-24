using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    public struct CohesionJob : IJobParallelFor
    {
        private const float CoverageRadius = 5.0f;
        private const float ThresholdRadius = 2.0f;

        private float _positionsCount;
        
        [NativeDisableParallelForRestriction] private NativeArray<Vector3> _positions;
        private NativeArray<Vector3> _accelerations;

        public CohesionJob(float positionsCount, NativeArray<Vector3> positions, NativeArray<Vector3> accelerations)
        {
            _positionsCount = positionsCount;
            _positions = positions;
            _accelerations = accelerations;
        }

        public void Execute(int index)
        {
            Vector3 averagePosition = Vector3.zero;
            Vector3 averageAvoidPosition = Vector3.zero;
            Vector3 self = _positions[index];
            int neighboursCount = 0;
            int avoidableNeighboursCount = 0;
            
            for (int i = 0; i < _positionsCount; i++)
            {
                if (i == index)
                {
                    continue;
                }
                
                
                Vector3 other = _positions[i];

                if (Vector3.Distance(self, other) < ThresholdRadius)
                {
                    averageAvoidPosition += self - other;
                    avoidableNeighboursCount++;
                }
                else if (Vector3.Distance(self, other) < CoverageRadius)
                {
                    averagePosition += other;
                    neighboursCount++;
                }
            }

            if (neighboursCount > 0)
            {
                averagePosition /= neighboursCount;
                _accelerations[index] += averagePosition - self;
            }
            
            if (avoidableNeighboursCount > 0)
            {
                _accelerations[index] += averageAvoidPosition / avoidableNeighboursCount;
            }

        }
    }
}