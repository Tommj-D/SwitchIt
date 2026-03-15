using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Dissolve : MonoBehaviour
{
    [Header("Dissolve Settings")]
    [SerializeField] private float dissolveTime = 0.75f;
    [SerializeField] private bool useVertical = false;
    [SerializeField] private bool destroyAfterDissolve = true;

    [Header("Shader Settings")]
    [SerializeField] private float outlineThickness = 0.1f;
    [SerializeField] private float dissolveScale = 30f;
    [ColorUsage(true, true)][SerializeField] private Color outlineColor_Real = Color.white;
    [ColorUsage(true, true)][SerializeField] private Color outlineColor_Fantasy = Color.cyan;
    [SerializeField] private float spiralStrength = 5f;

    private Material[] materials;
    private int dissolveAmount = Shader.PropertyToID("_DissolveAmount");
    private int verticalDissolve = Shader.PropertyToID("_VerticalDissolve");
    private int outlineThicknessID = Shader.PropertyToID("_OutlineThickness");
    private int outlineColorID = Shader.PropertyToID("_OutlineColor");
    private int spiralStrengthID = Shader.PropertyToID("_SpiralStrength");
    private int dissolveScaleID = Shader.PropertyToID("_DissolveScale");

    private void Awake()
    {
        RefreshRenderers();
        SetDissolve(0f);
    }

    // Questa funzione prende TUTTI i renderer sotto questo oggetto e figli
    public void RefreshRenderers()
    {
        // SpriteRenderer
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        // TilemapRenderer
        TilemapRenderer tilemapRenderer = GetComponentInChildren<TilemapRenderer>(true);

        int total = spriteRenderers.Length + (tilemapRenderer != null ? 1 : 0);
        materials = new Material[total];
        int index = 0;

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            // Crea una copia del materiale per sicurezza
            materials[index] = new Material(sr.material);
            sr.material = materials[index];
            index++;
        }

        if (tilemapRenderer != null)
        {
            materials[index] = new Material(tilemapRenderer.material);
            tilemapRenderer.material = materials[index];
        }

        ApplyShaderSettings();
    }

    private void ApplyShaderSettings()
    {
        foreach (Material mat in materials)
        {
            if (mat.HasProperty(outlineThicknessID)) mat.SetFloat(outlineThicknessID, outlineThickness);
            if (mat.HasProperty(outlineColorID))
            {
                bool fantasy = WorldSwitch.Instance != null && WorldSwitch.Instance.isFantasyWorldActive;
                mat.SetColor(outlineColorID, fantasy ? outlineColor_Fantasy : outlineColor_Real);
            }
            if (mat.HasProperty(spiralStrengthID)) mat.SetFloat(spiralStrengthID, spiralStrength);
            if (mat.HasProperty(dissolveScaleID)) mat.SetFloat(dissolveScaleID, dissolveScale);
        }
    }

    public void DissolveObject()
    {
        RefreshRenderers(); // Aggiorna renderer appena prima della dissolvenza
        StartCoroutine(DissolveRoutine(0f, 1.1f));
    }

    public void AppearObject()
    {
        RefreshRenderers();
        SetDissolve(1f);
        StartCoroutine(DissolveRoutine(1.1f, 0f));
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
        if (materials == null) return;

        foreach (Material mat in materials)
        {
            if (mat.HasProperty(dissolveAmount)) mat.SetFloat(dissolveAmount, value);
            if (useVertical && mat.HasProperty(verticalDissolve)) mat.SetFloat(verticalDissolve, value);
        }
    }

    public void UpdateDissolveColor()
    {
        ApplyShaderSettings(); 
    }

    // Metdodo chimaato da PuzzleController per aspettare il tepo giusto durante la dissolvenza
    public float GetDissolveTime()
    {
        return dissolveTime;
    }
}