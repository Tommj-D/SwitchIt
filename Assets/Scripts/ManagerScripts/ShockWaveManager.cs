using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShockWaveManager : MonoBehaviour
{
    public static ShockWaveManager Instance;

    public float shockWaveTime = 0.75f;

    private Coroutine shockWaveCoroutine;
    private Material _material;
    private static int waveDistanceFromCenter = Shader.PropertyToID("_WaveDistanceFromCenter");
    private static int waveCenterID = Shader.PropertyToID("_RingSpawnPosition");
    private Camera _cam;

    private void Awake()
    {
        Instance = this;
        _cam = Camera.main;
        _material = GetComponent<SpriteRenderer>().material;
    }
    public void CallShockWave(Vector3 worldPosition)
    {
        Vector3 screenPos = _cam.WorldToViewportPoint(worldPosition);

        if (screenPos.z < 0)
            return;

        _material.SetVector(waveCenterID,
            new Vector4(screenPos.x, screenPos.y, 0, 0));

        if (shockWaveCoroutine != null)
            StopCoroutine(shockWaveCoroutine);

        shockWaveCoroutine = StartCoroutine(
            ShockWaveAction(-0.1f, 1f));
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
