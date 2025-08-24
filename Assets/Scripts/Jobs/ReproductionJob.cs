﻿using Constants;
using Models;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    [BurstCompile]
    public struct ReproductionJob : IJobParallelFor
    {
        [ReadOnly, NativeDisableParallelForRestriction] 
        private NativeArray<Vector3> _positions;
        
        [ReadOnly, NativeDisableParallelForRestriction] 
        private NativeArray<PointOfInterest> _pointsOfInterest;
        
        [NativeDisableParallelForRestriction] 
        private NativeArray<byte> _reproductionResults;
        
        private readonly float _entitiesCount;
        private readonly float _reproductionZoneRadius;
        private readonly float _reproductionContactRadius;

        public ReproductionJob(
            float entitiesCount, 
            NativeArray<Vector3> positions, 
            NativeArray<PointOfInterest> pointsOfInterest, 
            NativeArray<byte> reproductionResults, 
            float reproductionZoneRadius, 
            float reproductionContactRadius)
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
            if (_reproductionResults[index] == Bytes.True)
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
                    _reproductionResults[index] = _reproductionResults[i] = Bytes.True;
                    break;
                }
            }
        }
    }
}