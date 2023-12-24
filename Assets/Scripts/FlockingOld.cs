using System.Collections.Generic;
using System.Linq;
using BoidsLogic;
using Jobs;
using Models;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class FlockingOld : MonoBehaviour
{
    [SerializeField] private GameObject _entityPrefab;
    [SerializeField] private float _entitiesVelocityLimit;
    [SerializeField] private int _sourceEntitiesCount = 50;
    [SerializeField] float _density = 0.15f;
    [SerializeField] private Bounds _entitiesMovingBounds;
    //[SerializeField] private Transform _pointOfInterest;
    
    private int _entitiesCount;
    
    private int _maxEntitiesCount = 1000;
    
    private TransformAccessArray _transformAccessArray;
    
    private NativeArray<Vector3> _entitiesPositions;
    private NativeArray<Vector3> _entitiesVelocities;
    private NativeArray<Vector3> _entitiesAccelerations;

    private Transform[] _entitiesTransforms;

    private NativeArray<PointOfInterest> _pointsOfInterest;

    private InterestsManager _interestsManager;
    
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
        Debug.Log("Gotten array lenght: " + _pointsOfInterest.Length);
    }

    private void Start()
    {
        _entitiesCount = _sourceEntitiesCount;
        _entitiesTransforms = new Transform[_maxEntitiesCount];

        for (int i = 0; i < _entitiesCount; i++)
        {
            GameObject entity = Instantiate(_entityPrefab, 
                Random.insideUnitSphere * _sourceEntitiesCount * _density,
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
            _entitiesVelocities[i] = Random.insideUnitSphere;
        }
        
        _entitiesAccelerations = new NativeArray<Vector3>(_maxEntitiesCount, Allocator.Persistent);
    }

    [EasyButtons.Button]
    private void Add3()
    {
        Reproduction(3);
    }
    
    private void Reproduction(int entitiesCountAppend)
    {
        int newEntitiesCount = Mathf.Clamp(_entitiesCount + entitiesCountAppend, 0, _maxEntitiesCount);
        
        for (int i = _entitiesCount; i < newEntitiesCount; i++)
        {
            GameObject entity = Instantiate(_entityPrefab, 
                Random.insideUnitSphere * entitiesCountAppend * _density,
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
    }


    private void Update()
    {
        /*foreach (Vector3 position in _entitiesPositions)
        {
            if (Vector3.Distance(position, _pointOfInterest.position) <= 1.0f)
            {
                _pointOfInterest.position = Random.insideUnitSphere * 15.0f;
                break;
            }
        }*/

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
        
        Debug.Log("count: " + _entitiesCount);

        JobHandle boundsJobHandler = boundsJob.Schedule(_entitiesCount, 4);
        
        JobHandle cohesionJobHandle = cohesionJob.Schedule(_entitiesCount, 4, boundsJobHandler); 
        
        JobHandle moveJobHandle = moveJob.Schedule(_transformAccessArray, cohesionJobHandle);

        JobHandle pointOfInterestJobHandle = pointOfInterestJob.Schedule(_entitiesCount, 4, moveJobHandle);
        pointOfInterestJobHandle.Complete();
        
        
    }
    

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(_entitiesMovingBounds.center, _entitiesMovingBounds.size);
    }
    
    
    private void OnDestroy()
    {
        _pointsOfInterest.Dispose();
        
        _entitiesVelocities.Dispose();
        _entitiesAccelerations.Dispose();
        _entitiesPositions.Dispose();
        
        _transformAccessArray.Dispose();
    }
}