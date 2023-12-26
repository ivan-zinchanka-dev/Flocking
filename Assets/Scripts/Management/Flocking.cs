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
        [SerializeField] private GameObject _entityPrefab;
        [SerializeField] private int _sourceEntitiesCount = 50;
        [SerializeField] float _density = 0.15f;
        [SerializeField] private Bounds _entitiesMovingBounds;
        [field:SerializeField, Range(1.0f, 10.0f)] public float EntityVelocityLimit { get; set; } = 10.0f;
        [field:SerializeField, Range(0.25f, 4.0f)] public float ReproductionRate { get; set; } = 1.0f;
    
        private const int MaxEntitiesCount = 1000;
        private int _entitiesCount;
        public int EntitiesCount => _entitiesCount;
        public event Action<int> OnEntitiesCountChanged; 
    
        private Transform[] _entitiesTransforms;
        private TransformAccessArray _transformAccessArray;
        private NativeArray<PointOfInterest> _pointsOfInterest;
    
        private NativeArray<Vector3> _entitiesPositions;
        private NativeArray<Vector3> _entitiesVelocities;
        private NativeArray<Vector3> _entitiesAccelerations;
        private NativeArray<byte> _reproductionResults;
    
        private readonly CancellationTokenSource _reproductionCts = new CancellationTokenSource();
        private InterestsManager _interestsManager;
        private float _cachedMinExtent = -1.0f;
        
        public void Initialize(InterestsManager interestsManager)
        {
            _interestsManager = interestsManager;
        }
        
        private GameObject CreateEntity()
        {
            if (_cachedMinExtent < 0.0f)
            {
                Vector3 boundsExtents = _entitiesMovingBounds.extents;
                _cachedMinExtent = Mathf.Min(boundsExtents.x, boundsExtents.y, boundsExtents.z);
            }
            
            return Instantiate(_entityPrefab, 
                Random.insideUnitSphere * Mathf.Clamp(_entitiesCount * _density, 0.0f, _cachedMinExtent),
                Quaternion.Euler(Vector3.forward * Random.Range(0.0f, 360.0f)), 
                transform);
        }

        private Vector3 GetStartVelocity()
        {
            return Random.insideUnitSphere * Mathf.Sqrt(EntityVelocityLimit);
        }

        private static int GetInnerLoopBatchCount(int arrayLength)
        {
            return Mathf.CeilToInt(arrayLength / 4.0f);
        }

        private void Start()
        {
            Debug.Log("True Byte: " + Convert.ToByte(true));
            Debug.Log("False Byte: " + Convert.ToByte(false));
            
            _entitiesCount = _sourceEntitiesCount;
            _entitiesTransforms = new Transform[MaxEntitiesCount];

            for (int i = 0; i < _entitiesCount; i++)
            {
                GameObject entity = CreateEntity();
                entity.name = "entity_" + i;
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
                if (ReproductionRate == 0)
                {
                    continue;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(1.0f / ReproductionRate));
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
                GameObject entity = CreateEntity();
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
                _entitiesMovingBounds.size);
            
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
                EntityVelocityLimit);
            
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
            Gizmos.DrawWireCube(_entitiesMovingBounds.center, _entitiesMovingBounds.size);
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