using System;
using System.Collections.Generic;
using System.Numerics;
using Jobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class Flocking : MonoBehaviour
{
    [SerializeField] private GameObject _entityPrefab;
    
    [SerializeField] private int _sourceEntitiesCount = 50;

    private const float Density = 0.08f;
    private int _entitiesCount;
    
    private TransformAccessArray _transformAccessArray;
    
    private NativeArray<Vector3> _entitiesPositions;

    private void Start()
    {
        Transform[] entitiesTransforms = new Transform[_sourceEntitiesCount];

        for (int i = 0; i < entitiesTransforms.Length; i++)
        {
            GameObject entity = Instantiate(_entityPrefab, 
                Random.insideUnitSphere * _sourceEntitiesCount * Density,
                Quaternion.Euler(Vector3.forward * Random.Range(0.0f, 360.0f)), 
                transform);

            entity.name = "entity_" + i;
            entitiesTransforms[i] = entity.transform;
        }

        _transformAccessArray = new TransformAccessArray(entitiesTransforms);
        _entitiesCount = _sourceEntitiesCount;
        
        
        _entitiesPositions = new NativeArray<Vector3>(_entitiesCount, Allocator.Persistent);
        
        for (int i = 0; i < _entitiesPositions.Length; i++)
        {
            _entitiesPositions[i] = _transformAccessArray[i].position;
            Debug.Log("io: " + _entitiesPositions[i]);
        }
        
    }


    private void Update()
    {
        DetectNearbyEntitiesJob detectNearbyJob = new DetectNearbyEntitiesJob(1.5f);
        //MotionJob motionJob = new MotionJob(_entitiesPositions);
        
        JobHandle detectNearbyJobHandle = detectNearbyJob.Schedule(_entitiesCount, 0);
        //JobHandle motionJobHandle = motionJob.Schedule(_transformAccessArray, detectNearbyJobHandle);
        
        detectNearbyJobHandle.Complete();
    }
    
    private void OnDestroy()
    {
        _entitiesPositions.Dispose();
        
        _transformAccessArray.Dispose();
    }
}