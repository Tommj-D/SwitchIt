using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class LightEffect_Shader : MonoBehaviour
{
    [SerializeField] private float _duration = 0.25f;

    private int _hitEffectAmount = Shader.PropertyToID("_HitEffectAmount");

    private SpriteRenderer[] _spriteRenderers;
    private Material[] _materials;

    private void Update()
    {
        if(Keyboard.current.fKey.wasPressedThisFrame)
        {
            StartCoroutine(ApplyLightEffect());
        }
    }
    private void Awake()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        _materials = new Material[_spriteRenderers.Length];

        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i] = _spriteRenderers[i].material;
        }
    }

    private IEnumerator ApplyLightEffect()
    {
        float elapsedTime = 0f;

        while (elapsedTime < _duration)
        {
            elapsedTime += Time.deltaTime;

            float lerpedAmount = Mathf.Lerp(0f, 1f, (elapsedTime / _duration));
            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i].SetFloat(_hitEffectAmount, lerpedAmount);
            }

            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < _duration)
        {
            elapsedTime += Time.deltaTime;

            float lerpedAmount = Mathf.Lerp(1f, 0f, (elapsedTime / _duration));
            for (int i = 0; i < _materials.Length; i++)
            {
                _materials[i].SetFloat(_hitEffectAmount, lerpedAmount);
            }

            yield return null;
        }
    }
}
