using UnityEngine;
using System.Collections;

public class MinionAppear : MonoBehaviour
{
    //==================================================
    // 🌫 IMPOSTAZIONI DISSOLVE & SHADER
    //==================================================
    [Header("Impostazioni Dissolve")]
    [SerializeField] private float tempoDiComparsa = 0.5f;
    [SerializeField] private bool useVerticalDissolve = false;

    [Header("Shader Settings")]
    [SerializeField] private float outlineThickness = 0.1f;
    [SerializeField] private float dissolveScale = 30f;

    [ColorUsage(true, true)] // Permette l'utilizzo di colori HDR (luminosi/neon)
    [SerializeField] private Color outlineColor = Color.white;

    [SerializeField] private float spiralStrength = 5f;

    // Array per memorizzare i materiali unici di questo minion
    private Material[] dissolveMaterials;

    // ID numerici delle proprietà dello shader (ottimizzazione delle prestazioni)
    private int dissolveAmountID = Shader.PropertyToID("_DissolveAmount");
    private int verticalDissolveID = Shader.PropertyToID("_VerticalDissolve");
    private int outlineThicknessID = Shader.PropertyToID("_OutlineThickness");
    private int outlineColorID = Shader.PropertyToID("_OutlineColor");
    private int spiralStrengthID = Shader.PropertyToID("_SpiralStrength");
    private int dissolveScaleID = Shader.PropertyToID("_DissolveScale");

    //==================================================
    // INIZIALIZZAZIONE
    //==================================================
    private void Start()
    {
        // 1. Recuperiamo tutti i componenti SpriteRenderer sul minion (anche nei figli)
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        dissolveMaterials = new Material[renderers.Length];

        // 2. Creiamo un'istanza unica del materiale per ogni sprite, così non influenziamo gli altri minion
        for (int i = 0; i < renderers.Length; i++)
        {
            dissolveMaterials[i] = new Material(renderers[i].material);
            renderers[i].material = dissolveMaterials[i];
        }

        // 3. Applichiamo le configurazioni estetiche dello shader appena il minion nasce
        ApplyShaderSettings();

        // 4. Facciamo partire l'effetto visivo di comparsa (da invisibile a visibile)
        StartCoroutine(AppearRoutine());
    }

    //==================================================
    // APPLICAZIONE PARAMETRI SHADER
    //==================================================
    private void ApplyShaderSettings()
    {
        if (dissolveMaterials == null) return;

        // Cicliamo su ogni materiale del minion per assegnare i valori dell'Inspector
        foreach (Material mat in dissolveMaterials)
        {
            if (mat.HasProperty(outlineThicknessID))
                mat.SetFloat(outlineThicknessID, outlineThickness);

            if (mat.HasProperty(outlineColorID))
                mat.SetColor(outlineColorID, outlineColor);

            if (mat.HasProperty(spiralStrengthID))
                mat.SetFloat(spiralStrengthID, spiralStrength);

            if (mat.HasProperty(dissolveScaleID))
                mat.SetFloat(dissolveScaleID, dissolveScale);
        }
    }

    //==================================================
    // GESTIONE VALORE DISSOLVE (COROUTINE)
    //==================================================
    private void SetDissolveAmount(float value)
    {
        if (dissolveMaterials == null) return;

        foreach (Material mat in dissolveMaterials)
        {
            if (mat.HasProperty(dissolveAmountID))
                mat.SetFloat(dissolveAmountID, value);

            // Se 'useVerticalDissolve' è vero, applichiamo il valore, altrimenti lasciamo a 0
            if (mat.HasProperty(verticalDissolveID))
                mat.SetFloat(verticalDissolveID, useVerticalDissolve ? value : 0f);
        }
    }

    private IEnumerator AppearRoutine()
    {
        float elapsed = 0f;

        // Impostiamo il valore iniziale (1.1 significa completamente dissolto/invisibile)
        SetDissolveAmount(1.1f);

        // Nel tempo, riduciamo il valore verso lo 0 (completamente visibile)
        while (elapsed < tempoDiComparsa)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / tempoDiComparsa;
            
            // Mathf.Lerp fa una transizione fluida tra il punto di partenza (1.1) e quello di arrivo (0)
            float value = Mathf.Lerp(1.1f, 0f, t);

            SetDissolveAmount(value);
            yield return null;
        }

        // Sicurezza finale: impostiamo il valore a 0 per mostrare perfettamente lo sprite
        SetDissolveAmount(0f);
    }
}