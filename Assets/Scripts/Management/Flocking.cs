using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Jobs;
using Models;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

namespace Management
{
    public class Flocking : MonoBehaviour
    {
        [SerializeField] 
        private FlockingConfig _config;
        
        [SerializeField] 
        private FlockingEntityFactory _entityFactory;
        
        [SerializeField]
        private InterestsManager _interestsManager;
        
        private const int MaxEntitiesCount = 1000;
        private int _entitiesCount;
        private readonly CancellationTokenSource _reproductionCts = new();
        
        private Transform[] _entitiesTransforms;
        private TransformAccessArray _transformAccessArray;
        private NativeArray<PointOfInterest> _pointsOfInterest;
    
        private NativeArray<Vector3> _entitiesPositions;
        private NativeArray<Vector3> _entitiesVelocities;
        private NativeArray<Vector3> _entitiesAccelerations;
        private NativeArray<byte> _reproductionResults;
        
        public int EntitiesCount => _entitiesCount;
        public event Action<int> OnEntitiesCountChanged; 
        
        private static int GetInnerLoopBatchCount(int arrayLength)
        {
            return Mathf.CeilToInt(arrayLength / 4.0f);
        }

        private Vector3 GetStartVelocity()
        {
            return Random.insideUnitSphere * Mathf.Sqrt(_config.EntityVelocityLimit);
        }
        
        private void Start()
        {
            _entitiesCount = _config.SourceEntitiesCount;
            _entitiesTransforms = new Transform[MaxEntitiesCount];

            for (int i = 0; i < _entitiesCount; i++)
            {
                GameObject entity = _entityFactory.CreateEntity(_entitiesCount);
                entity.name = $"entity_{i}";
                _entitiesTransforms[i] = entity.transform;
            }

            _transformAccessArray = new TransformAccessArray(_entitiesTransforms);
            _entitiesPositions = new NativeArray<Vector3>(MaxEntitiesCount, Allocator.Persistent);
        
            for (int i = 0; i < _entitiesCount; i++)
            {
                _entitiesPositions[i] = _transformAccessArray[i].position;
            }

            _entitiesVelocities = new NativeArray<Vector3>(MaxEntitiesCount, Allocator.Persistent);
        
            for (int i = 0; i < _entitiesCount; i++)
            {
                _entitiesVelocities[i] = GetStartVelocity();
            }
        
            _entitiesAccelerations = new NativeArray<Vector3>(MaxEntitiesCount, Allocator.Persistent);
            _reproductionResults = new NativeArray<byte>(MaxEntitiesCount, Allocator.Persistent);
            _pointsOfInterest = _interestsManager.GetPointsOfInterest();
        
            OnEntitiesCountChanged?.Invoke(_entitiesCount);
            LaunchReproduction().Forget();
        }
        
        private async UniTaskVoid LaunchReproduction()
        {
            while (!_reproductionCts.IsCancellationRequested)
            {
                if (_config.ReproductionRate == 0)
                {
                    continue;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(1.0f / _config.ReproductionRate));
                TryMakeReproduction();
            }
        }
        
        private void TryMakeReproduction()
        {
            if (_entitiesCount >= MaxEntitiesCount)
            {
                return;
            }

            int entitiesCountAppend = 0;
        
            for (int i = 0; i < _reproductionResults.Length; i++)
            {
                if (Convert.ToBoolean(_reproductionResults[i]))
                {
                    entitiesCountAppend++;
                }

                _reproductionResults[i] = Convert.ToByte(false);
            }

            entitiesCountAppend /= 2;
            MakeReproduction(entitiesCountAppend);

            ReproductionJob reproductionJob = new ReproductionJob(
                _entitiesCount,
                _entitiesPositions,
                _pointsOfInterest,
                _reproductionResults,
                5.0f,
                1.0f);

            JobHandle reproductionJobHandle = reproductionJob.Schedule(_entitiesCount, 
                GetInnerLoopBatchCount(_entitiesCount));
            reproductionJobHandle.Complete();
        }
    
        private void MakeReproduction(int entitiesCountAppend)
        {
            int newEntitiesCount = Mathf.Clamp(_entitiesCount + entitiesCountAppend, 0, MaxEntitiesCount);
        
            for (int i = _entitiesCount; i < newEntitiesCount; i++)
            {
                GameObject entity = _entityFactory.CreateEntity(_entitiesCount);
                entity.name = $"entity_{i}_generated";
                _entitiesTransforms[i] = entity.transform;
            }

            for (int i = _entitiesCount; i < newEntitiesCount; i++)
            {
                _transformAccessArray[i] = _entitiesTransforms[i];
            }
        
            for (int i = _entitiesCount; i < newEntitiesCount; i++)
            {
                _entitiesPositions[i] = _transformAccessArray[i].position;
            }
        
            for (int i = _entitiesCount; i < newEntitiesCount; i++)
            {
                _entitiesVelocities[i] = GetStartVelocity();
            }

            _entitiesCount = newEntitiesCount;
            OnEntitiesCountChanged?.Invoke(_entitiesCount);
        }
    
        private void UpdatePointsOfInterest()
        {
            if (_pointsOfInterest.IsCreated)
            {
                _interestsManager.UpdatePointsOfInterest(_pointsOfInterest);
                _pointsOfInterest.Dispose();
            }
        
            _pointsOfInterest = _interestsManager.GetPointsOfInterest();
        }
        
        private void Update()
        {
            UpdatePointsOfInterest();

            BoundsJob boundsJob = new BoundsJob(
                _entitiesPositions, 
                _entitiesAccelerations, 
                _config.EntitiesMovingBounds.size);
            
            CohesionJob cohesionJob = new CohesionJob(
                _entitiesCount,
                5.0f, 
                2.0f, 
                _entitiesPositions, 
                _entitiesAccelerations);
            
            MoveJob moveJob = new MoveJob(
                _entitiesPositions, 
                _entitiesVelocities, 
                _entitiesAccelerations, 
                Time.deltaTime, 
                _config.EntityVelocityLimit);
            
            PointOfInterestJob pointOfInterestJob = new PointOfInterestJob(
                _pointsOfInterest, 
                _entitiesPositions, 
                _entitiesAccelerations, 
                1.0f, 
                0.1f);

            JobHandle boundsJobHandler = boundsJob.Schedule(_entitiesCount, 
                GetInnerLoopBatchCount(_entitiesCount));
            
            JobHandle cohesionJobHandle = cohesionJob.Schedule(_entitiesCount, 
                GetInnerLoopBatchCount(_entitiesCount), boundsJobHandler);
            
            JobHandle moveJobHandle = moveJob.Schedule(_transformAccessArray, cohesionJobHandle);
            
            JobHandle pointOfInterestJobHandle = pointOfInterestJob.Schedule(_entitiesCount, 
                GetInnerLoopBatchCount(_entitiesCount), moveJobHandle);
            pointOfInterestJobHandle.Complete();
        }
    
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(_config.EntitiesMovingBounds.center, _config.EntitiesMovingBounds.size);
        }
    
        private void OnDestroy()
        {
            _reproductionCts.Cancel();
            _reproductionCts.Dispose();
        
            _reproductionResults.Dispose();
            _entitiesVelocities.Dispose();
            _entitiesAccelerations.Dispose();
            _entitiesPositions.Dispose();
        
            _pointsOfInterest.Dispose();
            _transformAccessArray.Dispose();
        }
    }
}