using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace BoidsLogic
{
    public struct TryConsumeJob : IJob
    {
        private NativeArray<Vector3> _positions;
        private NativeArray<bool> _consumed;

        public TryConsumeJob(NativeArray<Vector3> positions, NativeArray<bool> consumed)
        {
            _positions = positions;
            _consumed = consumed;
        }

        public void Execute()
        {
            foreach (Vector3 position in _positions)
            {
                
            }
            
            
        }
    }
}