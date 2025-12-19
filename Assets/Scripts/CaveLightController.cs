using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CaveLightController : MonoBehaviour
{
    public Light2D globalLight;

    public float targetDarkIntensity = 0.2f;
    public float outsideIntensity = 1.0f;
    public float lerpSpeed = 2f;

    private float desiredIntensity;

    private static int playerInsideCaves = 0;

    void Start()
    {
        desiredIntensity = outsideIntensity;
        if (globalLight == null) Debug.LogWarning("Assegna Global Light2D.");
    }

    void Update()
    {
        if (globalLight)
        {
            globalLight.intensity = Mathf.MoveTowards(globalLight.intensity, desiredIntensity, lerpSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInsideCaves++;
            desiredIntensity = targetDarkIntensity;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            desiredIntensity = outsideIntensity;
            playerInsideCaves--;

        if (playerInsideCaves <= 0)
        {
            playerInsideCaves = 0;
            desiredIntensity = outsideIntensity;
        }
        }
    }
}
