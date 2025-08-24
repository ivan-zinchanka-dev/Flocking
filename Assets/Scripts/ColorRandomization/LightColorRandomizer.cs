using UnityEngine;

namespace ColorRandomization
{
    public class LightColorRandomizer : BaseColorRandomizer
    {
        [SerializeField] 
        private Light _light;
    
        private void Awake()
        {
            _light.color = GetRandomColor();
        }
    }
}