﻿using UnityEngine;

public class RandomizeLightColor : MonoBehaviour
{
    [SerializeField] private Light _light;
    [SerializeField] private Color _min;
    [SerializeField] private Color _max;
    
    private void Awake()
    {
        Color.RGBToHSV(_min, out var minH, out var minS, out var minV);
        Color.RGBToHSV(_max, out var maxH, out var maxS, out var maxV);
        
        Color randomColor = Random.ColorHSV(minH, maxH, minS, maxS, minV, maxV);
        _light.color = randomColor;
    }
}