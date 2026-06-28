using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class MenuPlayerAnimator : MonoBehaviour
{
    private Animator animator;

    [Header("Mondi (Sfondi del Menu)")]
    [Tooltip("Trascina qui l'oggetto che contiene lo sfondo reale")]
    public GameObject realWorld;
    [Tooltip("Trascina qui l'oggetto che contiene lo sfondo fantasy")]
    public GameObject fantasyWorld;

    [Header("Impostazioni Blink")]
    public float minBlinkTime = 3f;
    public float maxBlinkTime = 6f;
    private float nextBlinkTime;

    [Header("Impostazioni Onda Menu")]
    [Tooltip("Tempo minimo di attesa prima della prossima onda")]
    public float minWaveTime = 4f;
    [Tooltip("Tempo massimo di attesa prima della prossima onda")]
    public float maxWaveTime = 8f;
    private float nextWaveTime;
    
    public GameObject fantasyWavePrefab;
    public GameObject realWavePrefab;
    public float waveDuration = 0.5f;
    public float waveMaxScale = 20f;
    
    public Color fantasyWaveColor = new Color(0.8f, 0f, 1f, 1f);
    public Color realWaveColor = new Color(0.6f, 0.9f, 1f, 1f);

    [Header("Particelle")]
    public GameObject realToFantasyParticles;
    public GameObject fantasyToRealParticles;

    [Header("Player Light Effect")]
    [ColorUsage(true, true)] public Color realWorldPlayerColor = new Color(0.75f, 0.9f, 1f, 1f);
    [ColorUsage(true, true)] public Color fantasyWorldPlayerColor = new Color(0.85f, 0.75f, 1f, 1f);
    public float playerFadeInTime = 0.2f;
    public float playerFadeOutTime = 0.2f;
    [Range(0f, 1f)] public float MaxIntensity = 1f;

    // Stato iniziale: partiamo dal mondo reale
    private bool isFantasyWorldActive = false; 

    private void Start()
    {
        animator = GetComponent<Animator>();
        
        ImpostaProssimoBlink();
        ImpostaProssimaOnda();

        if (realWorld != null) realWorld.SetActive(true);
        if (fantasyWorld != null) fantasyWorld.SetActive(false);
    }

    private void Update()
    {
        // Gestione Blink
        if (Time.time >= nextBlinkTime)
        {
            animator.SetTrigger("Blink");
            ImpostaProssimoBlink();
        }

        // Gestione Cambio Dimensione
        if (Time.time >= nextWaveTime)
        {
            StartCoroutine(SpawnMenuWave());
            ImpostaProssimaOnda(); // Calcola il nuovo tempo casuale
        }
    }

    private void ImpostaProssimoBlink()
    {
        nextBlinkTime = Time.time + Random.Range(minBlinkTime, maxBlinkTime);
    }

    private void ImpostaProssimaOnda()
    {
        nextWaveTime = Time.time + Random.Range(minWaveTime, maxWaveTime);
    }

    private IEnumerator SpawnMenuWave()
    {
        bool goingToFantasy = !isFantasyWorldActive;

        // 1. Particelle
        SpawnParticles(goingToFantasy);

        // 2. Effetto Luce sul Player
        Color targetColor = goingToFantasy ? fantasyWorldPlayerColor : realWorldPlayerColor;
        
        if (LightEffect_Shader.Instance != null)
        {
            LightEffect_Shader.Instance.PlayEffect(targetColor, MaxIntensity, playerFadeInTime, playerFadeOutTime);
        }
        else
        {
            // Fallback di sicurezza: se l'Instance non è inizializzata ma lo script è sul player
            LightEffect_Shader localShader = GetComponent<LightEffect_Shader>();
            if (localShader != null)
            {
                localShader.PlayEffect(targetColor, MaxIntensity, playerFadeInTime, playerFadeOutTime);
            }
        }

        // 3. Preparazione Onda
        GameObject selectedPrefab = goingToFantasy ? fantasyWavePrefab : realWavePrefab;
        Color startColor = goingToFantasy ? fantasyWaveColor : realWaveColor;

        if (selectedPrefab == null) yield break;

        GameObject wave = Instantiate(selectedPrefab, transform.position, Quaternion.identity);
        wave.transform.localScale = Vector3.zero;

        SpriteRenderer sr = wave.GetComponent<SpriteRenderer>();
        float baseAlpha = sr != null ? sr.color.a : 1f;
        Color pureLightColor = new Color(0.95f, 0.98f, 1f, 1f);

        float elapsed = 0f;
        bool worldChanged = false;

        while (elapsed < waveDuration)
        {
            float t = elapsed / waveDuration;
            float eased = Mathf.Sin(t * Mathf.PI * 0.5f);

            wave.transform.localScale = Vector3.one * Mathf.Lerp(0f, waveMaxScale, eased);
            wave.transform.Rotate(0f, 0f, 400f * Time.deltaTime);

            if (sr != null)
            {
                float fade = Mathf.InverseLerp(0.5f, 1f, t);
                Color currentColor = Color.Lerp(startColor, pureLightColor, fade);
                float finalAlpha = Mathf.Lerp(baseAlpha, 0f, fade);
                sr.color = new Color(currentColor.r, currentColor.g, currentColor.b, finalAlpha);
            }

            // 4. Cambio Mondo esatto (a metà dell'onda)
            if (!worldChanged && t >= 0.45f)
            {
                CambiaSfondi(goingToFantasy);
                worldChanged = true;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(wave);
    }

    private void SpawnParticles(bool goingToFantasy)
    {
        GameObject selectedParticles = goingToFantasy ? realToFantasyParticles : fantasyToRealParticles;

        if (selectedParticles != null)
        {
            GameObject particles = Instantiate(selectedParticles, transform.position, Quaternion.identity);
            particles.transform.SetParent(transform);

            ParticleSystem ps = particles.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                Destroy(particles, ps.main.duration + ps.main.startLifetime.constantMax);
            }
        }
    }

    private void CambiaSfondi(bool toFantasy)
    {
        isFantasyWorldActive = toFantasy;

        if (realWorld != null) realWorld.SetActive(!isFantasyWorldActive);
        if (fantasyWorld != null) fantasyWorld.SetActive(isFantasyWorldActive);
    }
}