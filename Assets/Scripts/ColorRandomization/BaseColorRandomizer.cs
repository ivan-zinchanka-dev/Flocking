using UnityEngine;
using Random = UnityEngine.Random;

namespace ColorRandomization
{
    public abstract class BaseColorRandomizer : MonoBehaviour
    {
        [SerializeField] 
        private Color _min;
        
        [SerializeField] 
        private Color _max;

        protected Color GetRandomColor()
        {
            Color.RGBToHSV(_min, out float minH, out float minS, out float minV);
            Color.RGBToHSV(_max, out float maxH, out float maxS, out float maxV);
            
            return Random.ColorHSV(minH, maxH, minS, maxS, minV, maxV);
        }
    }
}