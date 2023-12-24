using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    public struct CohesionJob : IJobParallelFor
    {
        private const float CoverageRadius = 5.0f;
        private const float ThresholdRadius = 2.0f;
        
        [NativeDisableParallelForRestriction] private NativeArray<Vector3> _positions;
        private NativeArray<Vector3> _accelerations;
        
        public CohesionJob(NativeArray<Vector3> positions, NativeArray<Vector3> accelerations)
        {
            _positions = positions;
            _accelerations = accelerations;
        }
        
        public void Execute(int index)
        {
            Vector3 averagePosition = Vector3.zero;
            Vector3 self = _positions[index];
            int neighboursCount = 0;
            
            for (int i = 0; i < _positions.Length; i++)
            {
                if (i == index)
                {
                    continue;
                }
                
                
                Vector3 other = _positions[i];
                
                if (Vector3.Distance(self, other) < CoverageRadius)
                {
                    averagePosition += other;
                    neighboursCount++;
                }
            }

            averagePosition /= neighboursCount;
            _accelerations[index] += averagePosition - self;

        }
    }
}