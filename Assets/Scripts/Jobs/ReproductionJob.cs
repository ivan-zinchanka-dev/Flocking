using Models;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    public struct ReproductionJob : IJobParallelFor
    {
        private float _positionsCount;
        [NativeDisableParallelForRestriction] private NativeArray<Vector3> _positions;
        [NativeDisableParallelForRestriction] private NativeArray<PointOfInterest> _pointsOfInterest;
        [NativeDisableParallelForRestriction] private NativeArray<bool> _reproductionResults;
        
        private const float ReproductionZoneRadius = 5.0f;
        private const float ReproductionContactRadius = 1.0f;

        public ReproductionJob(float positionsCount, NativeArray<Vector3> positions,
            NativeArray<PointOfInterest> pointsOfInterest, NativeArray<bool> reproductionResults)
        {
            _positionsCount = positionsCount;
            _positions = positions;
            _pointsOfInterest = pointsOfInterest;
            _reproductionResults = reproductionResults;
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
                if (Vector3.Distance(selfPosition, _pointsOfInterest[i].Position) < ReproductionZoneRadius)
                {
                    insideReproductionZone = true;
                    break;
                }
            }

            if (!insideReproductionZone)
            {
                return;
            }

            for (int i = 0; i < _positionsCount; i++)
            {
                if (i == index)
                {
                    continue;
                }
                
                Vector3 otherPosition = _positions[i];
                
                if (Vector3.Distance(selfPosition, otherPosition) < ReproductionContactRadius)
                {
                    _reproductionResults[index] = true;
                    _reproductionResults[i] = true;
                    break;
                }
            }
            
        }
    }
}