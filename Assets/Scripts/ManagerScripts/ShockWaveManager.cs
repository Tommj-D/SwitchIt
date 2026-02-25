using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShockWaveManager : MonoBehaviour
{
    public float shockWaveTime = 0.75f;

    private Coroutine shockWaveCoroutine;
    private Material _material;
    private static int waveDistanceFromCenter = Shader.PropertyToID("_WaveDistanceFromCenter");

    private void Awake()
    {
        _material = GetComponent<SpriteRenderer>().material;
    }

    private void Update()
    {
        if(Keyboard.current.hKey.wasPressedThisFrame)
        {
            callShockWave();
        }
    }
    public void callShockWave()
    {
        if (shockWaveCoroutine != null)
        {
            StopCoroutine(shockWaveCoroutine);
        }
        shockWaveCoroutine = StartCoroutine(ShockWaveAction(-0.1f, 1f));
    }

    private IEnumerator ShockWaveAction(float startPos, float endPos)
    {
        _material.SetFloat(waveDistanceFromCenter, startPos);

        float lerpedAmount=0f;
        float elapsedTime=0f;

        while(elapsedTime<shockWaveTime)
        {
            elapsedTime += Time.deltaTime;
            lerpedAmount = Mathf.Lerp(startPos, endPos, (elapsedTime / shockWaveTime));
            _material.SetFloat(waveDistanceFromCenter, lerpedAmount);
            yield return null;
        }
    }
}
