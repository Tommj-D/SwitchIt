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

    /// <summary>
    /// Attiva l'effetto shockwave in un punto specifico del mondo, con una forza e una durata definite.
    /// </summary>
    /// <param name="worldPosition">Posizione da cui parte</param>
    /// <param name="strenght">Forza/grandezza</param>
    /// <param name="duration">Durata</param>
    public void CallShockWave(Vector3 worldPosition, float strenght, float duration)
    {
        Vector3 screenPos = _cam.WorldToViewportPoint(worldPosition);

        if (screenPos.z < 0)
        {
            Debug.LogWarning("il campo 'Z' dell'oggetto deve essere<0");
            return;
        }

        if(strenght<-5f||strenght>5f)
        {
            Debug.LogWarning("La forza dello shockwave deve essere compresa tra -5 e 5");
            return;
        }

        if(duration<=0f)
        {
            Debug.LogWarning("La durata dello shockwave deve essere maggiore di 0");
            return;
        }

        _material.SetVector(waveCenterID,
            new Vector4(screenPos.x, screenPos.y, 0, 0));

        _material.SetFloat("_ShockWaveStrenght", strenght);

        if (shockWaveCoroutine != null)
            StopCoroutine(shockWaveCoroutine);

        shockWaveCoroutine = StartCoroutine(
            ShockWaveAction(-0.1f, 1f, duration));
    }

    private IEnumerator ShockWaveAction(float startPos, float endPos, float duration)
    {   
        _material.SetFloat(waveDistanceFromCenter, startPos);

        shockWaveTime = duration;
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
