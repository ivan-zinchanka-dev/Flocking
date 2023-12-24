using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BoidsLogic;
using Jobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class FlockingOld : MonoBehaviour
{
    [SerializeField] private GameObject _entityPrefab;
    [SerializeField] private float _entitiesVelocityLimit;
    [SerializeField] private int _sourceEntitiesCount = 50;

    [SerializeField] private Transform _pointOfInterest;
    
    public float driveFactor = 10f;
    public float maxSpeed = 5f;
    
    private const float Density = 0.08f;
    private int _entitiesCount;
    
    private int _maxEntitiesCount = 1000;
    
    private TransformAccessArray _transformAccessArray;
    
    private NativeArray<Vector3> _entitiesPositions;
    private NativeArray<Vector3> _entitiesVelocities;
    private NativeArray<Vector3> _entitiesAccelerations;
    
    private NativeArray<SpherecastCommand> _detectionCommands;

    private Transform[] _entitiesTransforms; 
    
    private void Start()
    {
        _entitiesCount = _sourceEntitiesCount;
        _entitiesTransforms = new Transform[_maxEntitiesCount];

        for (int i = 0; i < _entitiesCount; i++)
        {
            GameObject entity = Instantiate(_entityPrefab, 
                Random.insideUnitSphere * _sourceEntitiesCount * Density,
                Quaternion.Euler(Vector3.forward * Random.Range(0.0f, 360.0f)), 
                transform);

            entity.name = "entity_" + i;
            _entitiesTransforms[i] = entity.transform;
        }

        _transformAccessArray = new TransformAccessArray(_entitiesTransforms);
        
        
        
        _entitiesPositions = new NativeArray<Vector3>(_maxEntitiesCount, Allocator.Persistent);
        
        for (int i = 0; i < _entitiesCount; i++)
        {
            Debug.Log("Iteration: " + i);
            
            _entitiesPositions[i] = _transformAccessArray[i].position;
        }

        _entitiesVelocities = new NativeArray<Vector3>(_maxEntitiesCount, Allocator.Persistent);
        
        for (int i = 0; i < _entitiesCount; i++)
        {
            _entitiesVelocities[i] = Random.insideUnitSphere;
        }
        
        _entitiesAccelerations = new NativeArray<Vector3>(_maxEntitiesCount, Allocator.Persistent);
    }
    
    private void Reproduction(int entitiesCountAppend)
    {
        int newEntitiesCount = _entitiesCount + entitiesCountAppend;
        
        for (int i = _entitiesCount; i < newEntitiesCount; i++)
        {
            GameObject entity = Instantiate(_entityPrefab, 
                Random.insideUnitSphere * entitiesCountAppend * Density,
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
        
    }


    private void Update()
    {
        
        /*for (int i = 0; i < _entitiesPositions.Length; i++)
        {
            NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(_entitiesCount, Allocator.Persistent);
            NativeArray<SpherecastCommand> command = new NativeArray<SpherecastCommand>(1, Allocator.Persistent);
            command[0] = _detectionCommands[i];
            
            JobHandle spherecastJobHandle = SpherecastCommand.ScheduleBatch(command, results, 1);       // only main thread
            spherecastJobHandle.Complete();

            foreach (var hit in results)
            {
                if (hit.collider != null)
                {
                    Debug.Log("hit");
                }
            }
            
            command.Dispose();
            results.Dispose();
        }*/

        //Detect();

        /*DetectNearbyEntitiesJob detectNearbyJob = new DetectNearbyEntitiesJob(1.5f, _entitiesPositions);
        //MotionJob motionJob = new MotionJob(_entitiesPositions);
        
        JobHandle detectNearbyJobHandle = detectNearbyJob.Schedule(_entitiesCount, 0);
        //JobHandle motionJobHandle = motionJob.Schedule(_transformAccessArray, detectNearbyJobHandle);
        
        detectNearbyJobHandle.Complete();*/
        
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
            _pointOfInterest.position);
        
        Debug.Log("count: " + _entitiesCount);

        JobHandle cohesionJobHandle = cohesionJob.Schedule(_entitiesCount, 4); 
        
        JobHandle moveJobHandle = moveJob.Schedule(_transformAccessArray, cohesionJobHandle);

        JobHandle pointOfInterestJobHandle = pointOfInterestJob.Schedule(_entitiesCount, 4, moveJobHandle);
        pointOfInterestJobHandle.Complete();
        
        /*foreach (Vector3 position in _entitiesPositions)
        {
            if (Vector3.Distance(position, _pointOfInterest.position) <= 1.0f)
            {
                _pointOfInterest.position = Random.insideUnitSphere * 30.0f;
                break;
            }
        }*/
    }

    private void Detect()
    {
        
        
        for (int i = 0; i < _entitiesPositions.Length; i++)
        {
            Collider[] results = new Collider[_entitiesCount];
            
            int count = Physics.OverlapSphereNonAlloc(_entitiesPositions[i], 13.5f, results);

            /*if (count > 0)
            {
                Debug.Log("detected");
            }*/

            List<Vector3> arr = new List<Vector3>(count);

            for (int j = 0; j < count; j++)
            {
                /*Gizmos.color = Color.red;
                Gizmos.DrawLine(_entitiesPositions[i], results[j].transform.position);*/

                if (results[i] != null)
                {
                    arr.Add(results[i].transform.position);
                }
            }
            
            Vector3 motion = CalculateCohesionMotion(_entitiesPositions[i], arr);
            
            
            motion *= driveFactor;
            
            if (motion.magnitude > maxSpeed)
            {
                motion = motion.normalized * maxSpeed;
            }
            
            _transformAccessArray[i].up = motion;
            _transformAccessArray[i].position += motion * Time.deltaTime;
            
            Debug.Log("motion: " + motion);
            
        }
        
    }

    private void OnDrawGizmos()
    {
        //Detect();
    }

    private Vector3 CalculateCohesionMotion(Vector3 entityPosition, List<Vector3> neighbours)
    {
        if (neighbours.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 cohesionMotion = Vector3.zero;

        foreach (Vector3 neighbourPosition in neighbours)
        {
            cohesionMotion += neighbourPosition;
        }

        cohesionMotion /= neighbours.Count;
        cohesionMotion -= entityPosition;

        return cohesionMotion;
    }


    private void OnDestroy()
    {
        //_detectionCommands.Dispose();

        _entitiesVelocities.Dispose();
        _entitiesAccelerations.Dispose();
        _entitiesPositions.Dispose();
        
        _transformAccessArray.Dispose();
    }
}