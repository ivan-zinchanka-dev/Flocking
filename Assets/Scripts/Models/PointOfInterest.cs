using System;
using UnityEngine;

namespace Models
{
    public struct PointOfInterest
    {
        public Guid Id;
        public Vector3 Position;
        public byte IsConsumed;

        public PointOfInterest(Guid id, Vector3 position, byte isConsumed = 0)
        {
            Id = id;
            Position = position;
            IsConsumed = isConsumed;
        }
    }
}