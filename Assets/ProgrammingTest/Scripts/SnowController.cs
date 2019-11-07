using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnowController : MonoBehaviour
{
    Renderer rend;
    public Slider heightSlider;
    public Slider heightFalloffSlider;
    public Slider slopeSlider;
    public Slider slopeFalloffSlider;
    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.shader = Shader.Find("Custom/Snow");
    }

    public void UpdateHeight()
    {
        rend.material.SetFloat("_Height", heightSlider.value);  
    }
    public void UpdateHeightFalloff()
    {
        rend.material.SetFloat("_HeightFalloff", heightFalloffSlider.value);
    }
    public void UpdateSlopeFalloff()
    {
        rend.material.SetFloat("_SlopeFalloff", slopeFalloffSlider.value);
    }
    public void UpdateSlope()
    {
        rend.material.SetFloat("_Slope", slopeSlider.value);
    }
}
