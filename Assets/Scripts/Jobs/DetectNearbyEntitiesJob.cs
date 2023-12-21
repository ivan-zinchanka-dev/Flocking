using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    public struct DetectNearbyEntitiesJob : IJobParallelFor
    {
        private NativeArray<Vector3> _positions;
        
        //private Collider[] _detectedEntities;
        
        public float DetectionRadius { get; private set; }

        public DetectNearbyEntitiesJob(float detectionRadius, NativeArray<Vector3> positions)
        {
            DetectionRadius = detectionRadius;
            Debug.Log("Rad: " + DetectionRadius);
            _positions = positions;
            
            //_detectedEntities = new Collider[100];
        }

        public void Execute(int index)
        {
            NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(2, Allocator.Temp);
            NativeArray<SpherecastCommand> commands = new NativeArray<SpherecastCommand>(1, Allocator.Temp);
            
            commands[0] = new SpherecastCommand(_positions[index], DetectionRadius, Vector3.zero, 0.0f);
            
            JobHandle handle = SpherecastCommand.ScheduleBatch(commands, results, 1);       // only main thread
            handle.Complete();

            foreach (RaycastHit hit in results)
            {
                if (hit.collider != null)
                {
                    Debug.Log("hit");
                    
                    // Do something with results
                }
            }

            results.Dispose();
            commands.Dispose();
            
            /*int count = Physics.OverlapSphereNonAlloc(_positions[index], DetectionRadius, _detectedEntities);
            Debug.Log("Detect: " + count);*/
        }
    }
}