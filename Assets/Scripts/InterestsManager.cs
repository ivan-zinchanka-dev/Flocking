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

    [SerializeField] private FlockingOld _flocking;

    private readonly Dictionary<Guid, Transform> _pointsOfInterest = new Dictionary<Guid, Transform> ();

    private void Awake()
    {
        for (int i = 0; i < 3; i++)
        {
            Transform pointOfInterest = Instantiate(_pointOfInterestPrefab, 
                Random.insideUnitSphere * _spawnBounds.extents.y, 
                Quaternion.identity, transform);

            Guid pointId = new Guid();
            _pointsOfInterest[pointId] = pointOfInterest;
        }

        _flocking.Initialize(this);
    }

    public NativeArray<PointOfInterest> GetPointsOfInterest()
    {
        PointOfInterest[] array = _pointsOfInterest
            .Select(point => new PointOfInterest(point.Key, point.Value.position))
            .ToArray();
        
        Debug.Log("Len: " + array.Length);
        // TODO always 1 
        
        return new NativeArray<PointOfInterest>(array, Allocator.Persistent);
    }

    public void UpdatePointsOfInterest(NativeArray<PointOfInterest> pointOfInterests)
    {
        for (int i = 0; i < pointOfInterests.Length; i++)
        {
            if (pointOfInterests[i].IsConsumed)
            {
                Debug.Log("Try destroy");
                
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