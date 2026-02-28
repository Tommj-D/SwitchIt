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
            _materials[i] = _spriteRenderers[i].material;
        }
    }

    /// <summary>
    /// Attiva l'effetto di luce sul player.
    /// </summary>
    /// <param name="color">Colore dell'effetto luminoso.</param>
    /// <param name="maxIntensity">Intensità massima raggiunta dall'effetto.</param>
    /// <param name="fadeInTime">Tempo in secondi per raggiungere l'intensità massima.</param>
    /// <param name="fadeOutTime">Tempo in secondi per tornare allo stato normale.</param>
    public void PlayEffect(Color color, float maxIntensity, float fadeInTime, float fadeOutTime)
    {
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        _currentCoroutine = StartCoroutine(
            ApplyLightEffect(color, maxIntensity, fadeInTime, fadeOutTime)
        );
    }

    private IEnumerator ApplyLightEffect(Color color, float maxIntensity, float fadeInTime, float fadeOutTime)
    {
        fadeInTime = Mathf.Max(0.01f, fadeInTime);
        fadeOutTime = Mathf.Max(0.01f, fadeOutTime);

        // Imposta colore
        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i].SetColor(_hitEffectColor, color);
        }

        float elapsed = 0f;

        // Fade IN
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(0f, maxIntensity, elapsed / fadeInTime);

            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i].SetFloat(_hitEffectAmount, t);
            }

            yield return null;
        }

        elapsed = 0f;

        // Fade OUT
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(maxIntensity, 0f, elapsed / fadeOutTime);

            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i].SetFloat(_hitEffectAmount, t);
            }

            yield return null;
        }

        // Assicura reset finale
        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i].SetFloat(_hitEffectAmount, 0f);
        }
    }
}