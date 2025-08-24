using UnityEngine;

namespace Models
{
    [CreateAssetMenu(fileName = "flocking_config", menuName = "Models/FlockingConfig", order = 0)]
    public class FlockingConfig : ScriptableObject
    {
        [field: SerializeField] 
        public float Density { get; private set; } = 0.15f;

        [field:SerializeField] 
        public int SourceEntitiesCount { get; private set; } = 50;
        
        [field:SerializeField]
        public Bounds EntitiesMovingBounds { get; private set; } = 
            new(Vector3.zero, new Vector3(50f, 35f, 50f));
        
        [field:SerializeField, Range(1.0f, 10.0f)] 
        public float EntityVelocityLimit { get; set; } = 10.0f;
        
        [field:SerializeField, Range(0.25f, 4.0f)] 
        public float ReproductionRate { get; set; } = 1.0f;
    }
}