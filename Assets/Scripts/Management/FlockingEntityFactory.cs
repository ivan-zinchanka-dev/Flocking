using Models;
using UnityEngine;

namespace Management
{
    public class FlockingEntityFactory : MonoBehaviour
    {
        [SerializeField]
        private GameObject _entityPrefab;
        
        [SerializeField]
        private FlockingConfig _flockingConfig;
        
        private float _cachedMinExtent = -1.0f;
        
        public GameObject CreateEntity(int entitiesCount)
        {
            if (_cachedMinExtent < 0.0f)
            {
                Vector3 boundsExtents = _flockingConfig.EntitiesMovingBounds.extents;
                _cachedMinExtent = Mathf.Min(boundsExtents.x, boundsExtents.y, boundsExtents.z);
            }
            
            return Instantiate(_entityPrefab, GetStartPosition(entitiesCount), GetStartRotation(), transform);
        }

        private Vector3 GetStartPosition(int entitiesCount)
        {
            return Random.insideUnitSphere * Mathf.Clamp(entitiesCount * _flockingConfig.Density, 0.0f, _cachedMinExtent);
        }

        private Quaternion GetStartRotation()
        {
            return Quaternion.Euler(Vector3.forward * Random.Range(0.0f, 360.0f));
        }
    }
}