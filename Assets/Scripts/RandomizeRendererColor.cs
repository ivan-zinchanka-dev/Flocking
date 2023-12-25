using UnityEngine;
using Random = UnityEngine.Random;

public class RandomizeRendererColor : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Color _min;
    [SerializeField] private Color _max;
    
    private void Awake()
    {
        Color.RGBToHSV(_min, out var minH, out var minS, out var minV);
        Color.RGBToHSV(_max, out var maxH, out var maxS, out var maxV);
        
        Color randomColor = Random.ColorHSV(minH, maxH, minS, maxS, minV, maxV);
        _renderer.material.color = randomColor;
    }
}