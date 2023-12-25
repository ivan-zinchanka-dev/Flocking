using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class InterestsManager : MonoBehaviour
{
    [SerializeField] private Transform _pointOfInterestPrefab;
    [SerializeField] private Bounds _spawnBounds;

    [SerializeField] private Flocking _flocking;

    private readonly Dictionary<Guid, Transform> _pointsOfInterest = new Dictionary<Guid, Transform> ();
    
    private void Awake()
    {
        for (int i = 0; i < 3; i++)
        {
            SpawnPointOfInterest();
        }

        _flocking.Initialize(this);
    }

    [EasyButtons.Button]
    public void SpawnPointOfInterest()
    {
        Transform pointOfInterest = Instantiate(_pointOfInterestPrefab, 
            Random.insideUnitSphere * _spawnBounds.extents.y * 0.85f, 
            Quaternion.identity, transform);

        Guid pointId = Guid.NewGuid();
        _pointsOfInterest[pointId] = pointOfInterest;
        
    }
    
    public NativeArray<PointOfInterest> GetPointsOfInterest()
    {
        return new NativeArray<PointOfInterest>(_pointsOfInterest
            .Select(point => new PointOfInterest(point.Key, point.Value.position))
            .ToArray(), Allocator.Persistent);
    }

    public void UpdatePointsOfInterest(NativeArray<PointOfInterest> pointOfInterests)
    {
        for (int i = 0; i < pointOfInterests.Length; i++)
        {
            if (pointOfInterests[i].IsConsumed)
            {
                Guid pointId = pointOfInterests[i].Id;
            
                if (_pointsOfInterest.TryGetValue(pointId, out Transform point))
                {
                    _pointsOfInterest.Remove(pointId);
                    Destroy(point.gameObject);
                }
            }
        }
    }


}