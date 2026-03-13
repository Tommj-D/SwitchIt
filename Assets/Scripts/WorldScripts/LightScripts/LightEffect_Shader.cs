using System.Collections;
using UnityEngine;

public class LightEffect_Shader : MonoBehaviour
{
    public static LightEffect_Shader Instance;

    private int _hitEffectAmount = Shader.PropertyToID("_HitEffectAmount");
    private int _hitEffectColor = Shader.PropertyToID("_HitEffectColor");

    private SpriteRenderer[] _spriteRenderers;
    private Material[] _materials;

    private Coroutine _currentCoroutine;

    private void Awake()
    {
        Instance = this;

        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        _materials = new Material[_spriteRenderers.Length];

        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i] = new Material(_spriteRenderers[i].material);
            _spriteRenderers[i].material = _materials[i];
        }
    }

    /// <summary>
    /// Attiva l'effetto di luce sul player.
    /// </summary>
    /// <param name="color">Colore dell'effetto luminoso.</param>
    /// <param name="maxIntensity">Intensità massima raggiunta dall'effetto.</param>
    /// <param name="fadeInTime">Tempo in secondi per raggiungere l'intensità massima.</param>
    /// <param name="fadeOutTime">Tempo in secondi per tornare allo stato normale.</param>
    /// <param name="target">GameObject su cui applicare l'effetto (opzionale)</param>
    public void PlayEffect(Color color, float maxIntensity, float fadeInTime, float fadeOutTime, GameObject target = null)
    {
        // Se non viene passato un target, usa il GameObject stesso
        SpriteRenderer[] spriteRenderers = target != null
            ? target.GetComponentsInChildren<SpriteRenderer>()
            : GetComponentsInChildren<SpriteRenderer>();

        Material[] materials = new Material[spriteRenderers.Length];
        for (int i = 0; i < materials.Length; i++)
            materials[i] = spriteRenderers[i].material;

        _currentCoroutine = StartCoroutine(
            ApplyLightEffect(color, maxIntensity, fadeInTime, fadeOutTime, materials)
        );
    }

    private IEnumerator ApplyLightEffect(Color color, float maxIntensity, float fadeInTime, float fadeOutTime, Material[] materials)
    {
        fadeInTime = Mathf.Max(0.01f, fadeInTime);
        fadeOutTime = Mathf.Max(0.01f, fadeOutTime);

        // Imposta colore
        for (int i = 0; i < materials.Length; i++)
            materials[i].SetColor(_hitEffectColor, color);

        float elapsed = 0f;

        // Fade IN
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, maxIntensity, elapsed / fadeInTime);
            for (int i = 0; i < materials.Length; i++)
                materials[i].SetFloat(_hitEffectAmount, t);

            yield return null;
        }

        elapsed = 0f;

        // Fade OUT
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(maxIntensity, 0f, elapsed / fadeOutTime);
            for (int i = 0; i < materials.Length; i++)
                materials[i].SetFloat(_hitEffectAmount, t);

            yield return null;
        }

        // Reset finale
        for (int i = 0; i < materials.Length; i++)
            materials[i].SetFloat(_hitEffectAmount, 0f);
    }
}