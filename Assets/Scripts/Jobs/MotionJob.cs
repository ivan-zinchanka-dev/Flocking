using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Jobs
{
    public struct MotionJob : IJobParallelForTransform
    {
        private NativeArray<Vector3> _positions;

        public MotionJob(NativeArray<Vector3> positions)
        {
            _positions = positions;
        }

        public void Execute(int index, TransformAccess transform)
        {
            transform.position = Vector3.one; // Random.insideUnitSphere * 5.0f;
            _positions[index] = transform.position;
            
            Debug.Log("Go");
        }
    }
}