using System;

namespace BoidsLogic
{
    [Serializable]
    public struct AccelerationWeights
    {
        public float AverageSpread;
        public float AverageVelocity;
        public float AveragePosition;
        public float PointOfInterest;
    }
}