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

        public AccelerationWeights(float averageSpread, float averageVelocity, float averagePosition, float pointOfInterest)
        {
            AverageSpread = averageSpread;
            AverageVelocity = averageVelocity;
            AveragePosition = averagePosition;
            PointOfInterest = pointOfInterest;
        }
    }
}