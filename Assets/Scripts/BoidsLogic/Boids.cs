using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

namespace BoidsLogic
{
    public class Boids : MonoBehaviour
    {
        [SerializeField] private GameObject _entityPrefab;
        [SerializeField] private int _entitiesNumber = 50;
        [SerializeField] private int _entitiesDestinationThreshold;
        [SerializeField] private float _entitiesVelocityLimit;
        [SerializeField] private AccelerationWeights _accelerationWeights;
        [SerializeField] private Transform _pointOfInterest;
        
        [SerializeField] private Bounds _entitiesMovingBounds;

        private NativeArray<Vector3> _entitiesPositions;
        private NativeArray<Vector3> _entitiesVelocities;
        private NativeArray<Vector3> _entitiesAccelerations;
        
        
        private TransformAccessArray _transformAccessArray;
        
        private void Start()
        {
            _entitiesPositions = new NativeArray<Vector3>(_entitiesNumber, Allocator.Persistent);
            _entitiesVelocities = new NativeArray<Vector3>(_entitiesNumber, Allocator.Persistent);
            _entitiesAccelerations = new NativeArray<Vector3>(_entitiesNumber, Allocator.Persistent);
            
            Transform[] entitiesTransforms = new Transform[_entitiesNumber];

            for (int i = 0; i < entitiesTransforms.Length; i++)
            {
                entitiesTransforms[i] = Instantiate(_entityPrefab, transform).transform;
                _entitiesVelocities[i] = Random.insideUnitSphere;
            }

            _transformAccessArray = new TransformAccessArray(entitiesTransforms);
        }

        private void Update()
        {
            /*if (_cons)
            {
                Debug.Log("Consumed");
            }*/


            BoundsJob boundsJob = new BoundsJob(
                _entitiesPositions, 
                _entitiesAccelerations, 
                _entitiesMovingBounds.size);
            
            AccelerationJob accelerationJob = new AccelerationJob(
                _entitiesPositions, 
                _entitiesVelocities,
                _entitiesAccelerations, 
                _entitiesDestinationThreshold, 
                _accelerationWeights);
            
            MoveJob moveJob = new MoveJob(
                _entitiesPositions, 
                _entitiesVelocities, 
                _entitiesAccelerations, 
                Time.deltaTime, 
                _entitiesVelocityLimit);

            PointOfInterestJob pointOfInterestJob = new PointOfInterestJob(
                _entitiesPositions, 
                _entitiesAccelerations,
                _accelerationWeights, 
                _pointOfInterest.position);
            
            
            
            JobHandle boundsJobHandle = boundsJob.Schedule(_entitiesNumber, 0);
            JobHandle accelerationJobHandle = accelerationJob.Schedule(_entitiesNumber, 0, boundsJobHandle);

            JobHandle pointOfInterestJobHandle = pointOfInterestJob.Schedule(_entitiesNumber, 0, accelerationJobHandle); 
            
            JobHandle moveJobHandle = moveJob.Schedule(_transformAccessArray, pointOfInterestJobHandle);
            moveJobHandle.Complete();

            foreach (Vector3 position in _entitiesPositions)
            {
                if (Vector3.Distance(position, _pointOfInterest.position) <= 1.0f)
                {
                    _pointOfInterest.position = Random.insideUnitSphere * 30.0f;
                    break;
                }
            }
            
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(_entitiesMovingBounds.center, _entitiesMovingBounds.size);
        }

        private void OnDestroy()
        {
            _entitiesPositions.Dispose();
            _entitiesVelocities.Dispose();
            _entitiesAccelerations.Dispose();
            _transformAccessArray.Dispose();
        }
    }
}