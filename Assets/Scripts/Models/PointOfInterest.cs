using System;
using UnityEngine;

namespace Models
{
    public struct PointOfInterest
    {
        public Guid Id;
        public Vector3 Position;
        public bool IsConsumed;

        public PointOfInterest(Guid id, Vector3 position, bool isConsumed = false)
        {
            Id = id;
            Position = position;
            IsConsumed = isConsumed;
        }

        public void Consume()
        {
            IsConsumed = true;
        }
    }
}