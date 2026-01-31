using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TorchFlicker : MonoBehaviour
{
    public Light2D light2D;
    public float intensityMin = 0.8f;
    public float intensityMax = 1.2f;
    public float speed = 5f;

    void Start()
    {
        if (light2D == null) light2D = GetComponent<Light2D>();
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * speed, 0.0f);
        light2D.intensity = Mathf.Lerp(intensityMin, intensityMax, noise);
    }
}
