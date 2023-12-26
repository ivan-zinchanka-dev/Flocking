using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    [BurstCompile]
    public struct CohesionJob : IJobParallelFor
    {
        private readonly float _entitiesCount;
        private readonly float _coverageRadius;
        private readonly float _thresholdRadius;
        
        [ReadOnly][NativeDisableParallelForRestriction] private NativeArray<Vector3> _positions;
        private NativeArray<Vector3> _accelerations;
        
        public CohesionJob(float entitiesCount, float coverageRadius, float thresholdRadius, NativeArray<Vector3> positions, 
            NativeArray<Vector3> accelerations)
        {
            _entitiesCount = entitiesCount;
            _coverageRadius = coverageRadius;
            _thresholdRadius = thresholdRadius;
            _positions = positions;
            _accelerations = accelerations;
        }

        public void Execute(int index)
        {
            Vector3 averagePosition = Vector3.zero;
            Vector3 averageAvoidPosition = Vector3.zero;
            Vector3 selfPosition = _positions[index];
            int neighboursCount = 0;
            int avoidableNeighboursCount = 0;
            
            for (int i = 0; i < _entitiesCount; i++)
            {
                if (i == index)
                {
                    continue;
                }
                
                Vector3 otherPosition = _positions[i];

                if (Vector3.Distance(selfPosition, otherPosition) < _thresholdRadius)
                {
                    averageAvoidPosition += selfPosition - otherPosition;
                    avoidableNeighboursCount++;
                }
                else if (Vector3.Distance(selfPosition, otherPosition) < _coverageRadius)
                {
                    averagePosition += otherPosition;
                    neighboursCount++;
                }
            }

            if (neighboursCount > 0)
            {
                averagePosition /= neighboursCount;
                _accelerations[index] += averagePosition - selfPosition;
            }
            
            if (avoidableNeighboursCount > 0)
            {
                _accelerations[index] += averageAvoidPosition / avoidableNeighboursCount;
            }

        }
    }
}