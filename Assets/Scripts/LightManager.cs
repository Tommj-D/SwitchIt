using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CaveLightManager : MonoBehaviour
{
    public static CaveLightManager Instance;

    public Light2D globalLight;
    public float insideIntensity = 0.2f;
    public float outsideIntensity = 1f;
    public float lerpSpeed = 2f;

    private int caveCounter = 0;
    private float targetIntensity;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        targetIntensity = outsideIntensity;
    }

    void Update()
    {
        globalLight.intensity = Mathf.MoveTowards(
            globalLight.intensity,
            targetIntensity,
            lerpSpeed * Time.deltaTime
        );
    }

    public void EnterCave()
    {
        caveCounter++;
        targetIntensity = insideIntensity;
    }

    public void ExitCave()
    {
        caveCounter--;

        if (caveCounter <= 0)
        {
            caveCounter = 0;
            targetIntensity = outsideIntensity;
        }
    }
}
