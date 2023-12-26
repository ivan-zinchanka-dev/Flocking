using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BoidsLogic;
using Cysharp.Threading.Tasks;
using Jobs;
using Models;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

public class Flocking : MonoBehaviour
{
    [SerializeField] private GameObject _entityPrefab;
    [SerializeField] private float _entitiesVelocityLimit;
    [SerializeField] private int _sourceEntitiesCount = 50;
    [SerializeField] float _density = 0.15f;
    [SerializeField] private Bounds _entitiesMovingBounds;
    [field: SerializeField, Range(0.25f, 4.0f)] public float ReproductionRate { get; set; } = 1.0f;
    
    private int _entitiesCount;
    
    private int _maxEntitiesCount = 1000;
    
    private TransformAccessArray _transformAccessArray;
    
    private NativeArray<Vector3> _entitiesPositions;
    private NativeArray<Vector3> _entitiesVelocities;
    private NativeArray<Vector3> _entitiesAccelerations;

    private Transform[] _entitiesTransforms;

    private NativeArray<PointOfInterest> _pointsOfInterest;
    private NativeArray<bool> _reproductionResults;


    private InterestsManager _interestsManager;

    private CancellationTokenSource _reproductionCts = new CancellationTokenSource();

    public int EntitiesCount => _entitiesCount;
    

    public event Action<int> OnEntitiesCountChanged; 

    public void Initialize(InterestsManager interestsManager)
    {
        _interestsManager = interestsManager;
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

    private void Start()
    {
        _entitiesCount = _sourceEntitiesCount;
        _entitiesTransforms = new Transform[_maxEntitiesCount];

        for (int i = 0; i < _entitiesCount; i++)
        {
            GameObject entity = Instantiate(_entityPrefab, 
                Random.insideUnitSphere * (_entitiesCount * _density),
                Quaternion.Euler(Vector3.forward * Random.Range(0.0f, 360.0f)), 
                transform);

            entity.name = "entity_" + i;
            _entitiesTransforms[i] = entity.transform;
        }

        _transformAccessArray = new TransformAccessArray(_entitiesTransforms);
        
        _entitiesPositions = new NativeArray<Vector3>(_maxEntitiesCount, Allocator.Persistent);
        
        for (int i = 0; i < _entitiesCount; i++)
        {
            _entitiesPositions[i] = _transformAccessArray[i].position;
        }

        _entitiesVelocities = new NativeArray<Vector3>(_maxEntitiesCount, Allocator.Persistent);
        
        for (int i = 0; i < _entitiesCount; i++)
        {
            _entitiesVelocities[i] = Random.insideUnitSphere * _entitiesVelocityLimit / 2;
        }
        
        _entitiesAccelerations = new NativeArray<Vector3>(_maxEntitiesCount, Allocator.Persistent);

        _reproductionResults = new NativeArray<bool>(_maxEntitiesCount, Allocator.Persistent);
        _pointsOfInterest = _interestsManager.GetPointsOfInterest();
        
        OnEntitiesCountChanged?.Invoke(_entitiesCount);
        
        LaunchReproduction().Forget();
    }

    private async UniTaskVoid LaunchReproduction()
    {
        while (!_reproductionCts.IsCancellationRequested)
        {
            //4 sec = 1 / 0.25
            if (ReproductionRate == 0)
            {
                continue;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(1.0f / ReproductionRate));
            TryMakeReproduction();
        }
    }

    [EasyButtons.Button]
    private void Add3()
    {
        MakeReproduction(3);
    }
    
    private void MakeReproduction(int entitiesCountAppend)
    {
        int newEntitiesCount = Mathf.Clamp(_entitiesCount + entitiesCountAppend, 0, _maxEntitiesCount);
        
        for (int i = _entitiesCount; i < newEntitiesCount; i++)
        {
            GameObject entity = Instantiate(_entityPrefab, 
                Random.insideUnitSphere * (_entitiesCount * _density),
                Quaternion.Euler(Vector3.forward * Random.Range(0.0f, 360.0f)), 
                transform);

            entity.name = $"entity_{i}_generated_";
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
            _entitiesVelocities[i] = Random.insideUnitSphere;
        }

        _entitiesCount = newEntitiesCount;
        
        OnEntitiesCountChanged?.Invoke(_entitiesCount);
    }


    private void Update()
    {
        UpdatePointsOfInterest();

        MoveJob moveJob = new MoveJob(
            _entitiesPositions, 
            _entitiesVelocities, 
            _entitiesAccelerations, 
            Time.deltaTime, 
            _entitiesVelocityLimit);

        CohesionJob cohesionJob = new CohesionJob(
            _entitiesCount,
            _entitiesPositions,
            _entitiesAccelerations);

        PointOfInterestJob pointOfInterestJob = new PointOfInterestJob(
            _entitiesPositions, 
            _entitiesAccelerations, 
            new AccelerationWeights(1.0f, 1.0f, 1.0f, 0.1f),
            Vector3.zero, 
            _pointsOfInterest);
        
        BoundsJob boundsJob = new BoundsJob(
            _entitiesPositions, 
            _entitiesAccelerations, 
            _entitiesMovingBounds.size);

        JobHandle boundsJobHandler = boundsJob.Schedule(_entitiesCount, 4);
        
        JobHandle cohesionJobHandle = cohesionJob.Schedule(_entitiesCount, 4, boundsJobHandler); 
        
        JobHandle moveJobHandle = moveJob.Schedule(_transformAccessArray, cohesionJobHandle);

        JobHandle pointOfInterestJobHandle = pointOfInterestJob.Schedule(_entitiesCount, 4, moveJobHandle);
        pointOfInterestJobHandle.Complete();
    }

    private void TryMakeReproduction()
    {
        if (_entitiesCount >= _maxEntitiesCount)
        {
            return;
        }

        int entitiesCountAppend = 0;
        
        for (int i = 0; i < _reproductionResults.Length; i++)
        {
            if (_reproductionResults[i])
            {
                entitiesCountAppend++;
            }

            _reproductionResults[i] = false;
        }

        entitiesCountAppend /= 2;
        MakeReproduction(entitiesCountAppend);

        ReproductionJob reproductionJob = new ReproductionJob(
            _entitiesCount,
            _entitiesPositions,
            _pointsOfInterest,
            _reproductionResults);

        JobHandle reproductionJobHandle = reproductionJob.Schedule(_entitiesCount, 4);
        reproductionJobHandle.Complete();
        
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
        _pointsOfInterest.Dispose();
        
        _entitiesVelocities.Dispose();
        _entitiesAccelerations.Dispose();
        _entitiesPositions.Dispose();
        
        _transformAccessArray.Dispose();
    }
}