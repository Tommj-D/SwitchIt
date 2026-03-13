using System.Collections;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float dissolveTime = 0.75f;
    [SerializeField] private bool destroyAfterDissolve = false;

    private SpriteRenderer[] spriteRenderers;
    private Material[] materials;

    private int dissolveAmount = Shader.PropertyToID("_DissolveAmount");
    private int verticalDissolve = Shader.PropertyToID("_VerticalDissolve");

    private Coroutine currentRoutine;

    void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        materials = new Material[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            materials[i] = spriteRenderers[i].material;
        }
    }

    // -------- PUBLIC FUNCTIONS --------

    public void DissolveObject(bool useVertical = false)
    {
        StartEffect(0f, 1f, useVertical);
    }

    public void AppearObject(bool useVertical = false)
    {
        StartEffect(1f, 0f, useVertical);
    }

    public void ToggleObject(bool useVertical = false)
    {
        float current = materials[0].GetFloat(dissolveAmount);

        if (current < 0.5f)
            DissolveObject(useVertical);
        else
            AppearObject(useVertical);
    }

    // -------- CORE --------

    private void StartEffect(float start, float end, bool useVertical)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(DissolveRoutine(start, end, useVertical));
    }

    private IEnumerator DissolveRoutine(float startValue, float endValue, bool useVertical)
    {
        float elapsed = 0f;

        while (elapsed < dissolveTime)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / dissolveTime;
            float value = Mathf.Lerp(startValue, endValue, t);

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].SetFloat(dissolveAmount, value);

                if (useVertical)
                    materials[i].SetFloat(verticalDissolve, value);
            }

            yield return null;
        }

        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat(dissolveAmount, endValue);

            if (useVertical)
                materials[i].SetFloat(verticalDissolve, endValue);
        }

        if (endValue == 1f && destroyAfterDissolve)
            Destroy(gameObject);
    }
}