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

        Vector2 corrected = new Vector2(screenPos.x, screenPos.y);

        //OFFSET CORRETTIVO (aggiustalo leggermente)
        corrected += new Vector2(0.003f, 0.006f); //altrimenti -0.01f,

        _material.SetVector(waveCenterID,
            new Vector4(corrected.x, corrected.y, 0, 0));

        _material.SetFloat("_ShockWaveStrenght", strenght);

        if (shockWaveCoroutine != null)
            StopCoroutine(shockWaveCoroutine);

        shockWaveCoroutine = StartCoroutine(
            ShockWaveAction(-0.1f, 1.5f, duration));
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

    void LateUpdate()
    {
        if (_cam == null) return;

        float distance = 2.2f;

        float height = 2f * Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance;
        float width = height * _cam.aspect;

        transform.position = _cam.transform.position + _cam.transform.forward * distance;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;

        transform.localScale = new Vector3(
            width / spriteWidth,
            height / spriteHeight,
            1f
        );
    }

    public void SetXSizeRatio(float value)
    {
        _material.SetFloat("_XSizeRatio", value);
    }
}
