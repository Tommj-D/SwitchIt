using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Dissolve : MonoBehaviour
{
    [Header("Dissolve Settings")]
    [SerializeField] private float dissolveTime = 0.75f;
    [SerializeField] private bool useVertical = true;
    [SerializeField] private bool destroyAfterDissolve = false;

    [Header("Shader Settings")]
    [SerializeField] private float outlineThickness = 0.1f;
    [SerializeField] private float dissolveScale = 30f;
    [ColorUsage(true, true)] [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float spiralStrength = 5f;
    
    private SpriteRenderer[] spriteRenderers;
    private TilemapRenderer tilemapRenderer;

    private Material[] materials;

    private int dissolveAmount = Shader.PropertyToID("_DissolveAmount");
    private int verticalDissolve = Shader.PropertyToID("_VerticalDissolve");

    private int outlineThicknessID = Shader.PropertyToID("_OutlineThickness");
    private int outlineColorID = Shader.PropertyToID("_OutlineColor");
    private int spiralStrengthID = Shader.PropertyToID("_SpiralStrength");
    private int dissolveScaleID = Shader.PropertyToID("_DissolveScale");

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        tilemapRenderer = GetComponent<TilemapRenderer>();

        int total = spriteRenderers.Length + (tilemapRenderer != null ? 1 : 0);
        materials = new Material[total];

        int index = 0;

        // SpriteRenderer
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            materials[index] = new Material(spriteRenderers[i].material);
            spriteRenderers[i].material = materials[index];
            index++;
        }

        // TilemapRenderer
        if (tilemapRenderer != null)
        {
            materials[index] = new Material(tilemapRenderer.material);
            tilemapRenderer.material = materials[index];
        }

        ApplyShaderSettings();
        SetDissolve(0f);
    }

    private void ApplyShaderSettings()
    {
        foreach (Material mat in materials)
        {
            mat.SetFloat(outlineThicknessID, outlineThickness);
            mat.SetColor(outlineColorID, outlineColor);
            mat.SetFloat(spiralStrengthID, spiralStrength);
            mat.SetFloat(dissolveScaleID, dissolveScale);
        }
    }

    public void DissolveObject()
    {
        StartCoroutine(DissolveRoutine(0f, 1f));
    }

    public void AppearObject()
    {
        SetDissolve(1f);
        StartCoroutine(DissolveRoutine(1f, 0f));
    }

    private IEnumerator DissolveRoutine(float start, float end)
    {
        float elapsed = 0f;

        while (elapsed < dissolveTime)
        {
            elapsed += Time.deltaTime;
            float value = Mathf.Lerp(start, end, elapsed / dissolveTime);

            SetDissolve(value);

            yield return null;
        }

        SetDissolve(end);

        if (end == 1f && destroyAfterDissolve)
            Destroy(gameObject);
    }

    private void SetDissolve(float value)
    {
        foreach (Material mat in materials)
        {
            mat.SetFloat(dissolveAmount, value);

            if (useVertical)
                mat.SetFloat(verticalDissolve, value);
        }
    }
}