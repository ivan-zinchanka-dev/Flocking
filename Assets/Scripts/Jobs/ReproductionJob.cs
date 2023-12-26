using Models;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    public struct ReproductionJob : IJobParallelFor
    {
        private readonly float _entitiesCount;
        [ReadOnly][NativeDisableParallelForRestriction] private NativeArray<Vector3> _positions;
        [ReadOnly][NativeDisableParallelForRestriction] private NativeArray<PointOfInterest> _pointsOfInterest;
        [NativeDisableParallelForRestriction] private NativeArray<bool> _reproductionResults;
        
        private readonly float _reproductionZoneRadius;
        private readonly float _reproductionContactRadius;

        public ReproductionJob(float entitiesCount, NativeArray<Vector3> positions, 
            NativeArray<PointOfInterest> pointsOfInterest, NativeArray<bool> reproductionResults, 
            float reproductionZoneRadius, float reproductionContactRadius)
        {
            _entitiesCount = entitiesCount;
            _positions = positions;
            _pointsOfInterest = pointsOfInterest;
            _reproductionResults = reproductionResults;
            _reproductionZoneRadius = reproductionZoneRadius;
            _reproductionContactRadius = reproductionContactRadius;
        }

        public void Execute(int index)
        {
            if (_reproductionResults[index])
            {
                return;
            }

            Vector3 selfPosition = _positions[index];
            bool insideReproductionZone = false;
            
            for (int i = 0; i < _pointsOfInterest.Length; i++)
            {
                if (Vector3.Distance(selfPosition, _pointsOfInterest[i].Position) < _reproductionZoneRadius)
                {
                    insideReproductionZone = true;
                    break;
                }
            }

            if (!insideReproductionZone)
            {
                return;
            }

            for (int i = 0; i < _entitiesCount; i++)
            {
                if (i == index)
                {
                    continue;
                }
                
                Vector3 otherPosition = _positions[i];
                
                if (Vector3.Distance(selfPosition, otherPosition) < _reproductionContactRadius)
                {
                    _reproductionResults[index] = true;
                    _reproductionResults[i] = true;
                    break;
                }
            }
            
        }
    }
}