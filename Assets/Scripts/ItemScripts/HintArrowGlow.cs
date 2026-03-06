using UnityEngine;
using UnityEngine.Tilemaps;

public class HintArrowGlow_Tilemap : MonoBehaviour
{
    [Header("Glow")]
    public float speed = 2f;
    public float minIntensity = 0.2f;
    public float maxIntensity = 0.6f;

    [ColorUsage(true, true)]
    public Color glowColor = Color.yellow;

    [Header("Floating")]
    public float floatAmplitude = 0.05f; // quanto si muove su/giù
    public float floatSpeed = 2f;        // velocità oscillazione

    int hitAmount = Shader.PropertyToID("_HitEffectAmount");
    int hitColor = Shader.PropertyToID("_HitEffectColor");

    Material mat;
    Vector3 startPos;

    void OnEnable()
    {
        TilemapRenderer tr = GetComponent<TilemapRenderer>();
        if (tr == null) return;

        mat = new Material(tr.material); // copia materiale per questa Tilemap
        tr.material = mat;

        mat.SetColor(hitColor, glowColor);
        mat.SetFloat(hitAmount, minIntensity);

        startPos = transform.localPosition; // salva posizione iniziale
    }

    void Update()
    {
        if (mat == null) return;

        // pulsazione glow
        float t = Mathf.Sin(Time.time * speed) * 0.5f + 0.5f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        mat.SetFloat(hitAmount, intensity);

        // floating Tilemap
        float y = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = startPos + new Vector3(0, y, 0);
    }
}