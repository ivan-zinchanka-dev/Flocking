using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Management
{
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

        private Vector3 GetPositionInsideBounds()
        {
            Vector3 min = _spawnBounds.min;
            Vector3 max = _spawnBounds.max;
            return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
        }

        [EasyButtons.Button]
        public void SpawnPointOfInterest()
        {
            Transform pointOfInterest = Instantiate(_pointOfInterestPrefab, 
                GetPositionInsideBounds(), 
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
                if (Convert.ToBoolean(pointOfInterests[i].IsConsumed))
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
}