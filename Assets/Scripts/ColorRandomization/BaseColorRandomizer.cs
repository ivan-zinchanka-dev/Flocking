using UnityEngine;
using Random = UnityEngine.Random;

namespace ColorRandomization
{
    public abstract class BaseColorRandomizer : MonoBehaviour
    {
        [SerializeField] private Color _min;
        [SerializeField] private Color _max;

        protected Color GetRandomColor()
        {
            Color.RGBToHSV(_min, out var minH, out var minS, out var minV);
            Color.RGBToHSV(_max, out var maxH, out var maxS, out var maxV);
            
            return Random.ColorHSV(minH, maxH, minS, maxS, minV, maxV);
        }
    }
}