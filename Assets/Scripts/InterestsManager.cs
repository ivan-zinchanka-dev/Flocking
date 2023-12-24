using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class InterestsManager : MonoBehaviour
{
    [SerializeField] private Transform _pointOfInterestPrefab;
    [SerializeField] private Bounds _spawnBounds;

    [SerializeField] private FlockingOld _flocking;

    public LinkedList<Transform> PointsOfInterest { get; private set; } = new LinkedList<Transform>();

    private void Awake()
    {
        for (int i = 0; i < 3; i++)
        {
            Transform pointOfInterest = Instantiate(_pointOfInterestPrefab, 
                Random.insideUnitSphere * _spawnBounds.extents.y, 
                Quaternion.identity, transform);

            PointsOfInterest.AddLast(pointOfInterest);
        }

        _flocking.Initialize(this);
    }

    

    
    

}