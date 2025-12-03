using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CaveLightController : MonoBehaviour
{
    public Light2D globalLight;

    public float targetDarkIntensity = 0.2f;
    public float outsideIntensity = 1.0f;
    public float lerpSpeed = 2f;

    float desiredIntensity;

    void Start()
    {
        desiredIntensity = outsideIntensity;
        if (globalLight == null) Debug.LogWarning("Assegna Global Light2D.");
    }

    void Update()
    {
        if (globalLight)
        {
            globalLight.intensity = Mathf.Lerp(globalLight.intensity, desiredIntensity, Time.deltaTime * lerpSpeed);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            desiredIntensity = targetDarkIntensity;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            desiredIntensity = outsideIntensity;
        }
    }
}
