using UnityEngine;

namespace ColorRandomization
{
    public class RendererColorRandomizer : BaseColorRandomizer
    {
        [SerializeField] private Renderer _renderer;
    
        private void Awake()
        {
            _renderer.material.color = GetRandomColor();
        }
    }
}