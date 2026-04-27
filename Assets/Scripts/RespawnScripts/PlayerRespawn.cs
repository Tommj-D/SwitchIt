using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class PlayerRespawn : MonoBehaviour
{
    private const string BACK_LAYER = "Back";
    private const string PLAYER_LAYER = "Player";
    private const string PLAYER_BACK_LAYER = "Player_Back";

    private const string PLAYER_SORTING = "Player";
    private const string PLAYER_BACK_SORTING = "PlayerBack";

    private PlayerInput playerInput;

    [Header("Magic Respawn")]
    private float spawnScale = 0.1f;
    public float growDuration = 0.6f;
    public float upwardForce = 8f;

    [Header("Respawn Timing")]
    public float blackScreenHoldTime = 0.2f;
    public float respawnDelay = 1.5f;
    
    public GameObject deathParticle;
    public GameObject fullSprite;      
    public GameObject riggedBody;

    [Header("ShockWave")]
    public float shockWaveDuration = 1f;
    [Range(-5f, 5f)]
    public float shockWaveStrenght = -0.1f;

    [Header("DeathLightEffect")]
    [ColorUsage(true, true)] public Color deathColor = new Color(0.75f, 0.9f, 1f, 1f);
    public float playerFadeInTime = 0.2f;
    public float playerFadeOutTime = 0.2f;
    [Range(0f, 1f)]
    public float MaxIntensity = 1f;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer fullSpriteRenderer;

    private bool isDying = false;
    private int defaultLayer;
    private string defaultSortingLayer;
    private int defaultSortingOrder;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        playerInput = GetComponent<PlayerInput>();

        if (fullSprite != null)
        {
            fullSpriteRenderer = fullSprite.GetComponent<SpriteRenderer>();
            defaultSortingLayer = fullSpriteRenderer.sortingLayerName;
            defaultSortingOrder = fullSpriteRenderer.sortingOrder;
        }

        defaultLayer = gameObject.layer;
    }

    private void Start()
    {
        // Se esiste il RespawnManager e il fullSprite, fai partire l'animazione di spawn
        if (RespawnManager.Instance != null && fullSpriteRenderer != null)
        {
            if (!GameManager.Instance.isTestMode)
                StartCoroutine(InitialSpawnSequence());
        }
    }

    private IEnumerator InitialSpawnSequence()
    {
        if (!GameManager.Instance.isTestMode)
        {
            yield return StartCoroutine(PrepareRespawn());
            yield return StartCoroutine(PlayRespawnAnimation());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDying && (collision.gameObject.CompareTag("Enemy")) || collision.gameObject.CompareTag("Spike"))
        {   
            StartCoroutine(DeathSequence());
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDying && other.gameObject.CompareTag("Death"))
        {
            StartCoroutine(DeathSequence());
        }
    }


    private IEnumerator DeathSequence()
    {
        if (isDying) yield break;  
        isDying = true;

        //Audio morte
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.playerDeathSound);
        }
        //Abbasso il volume mentre il player muore
        VolumeController.Instance.DuckMixer(VolumeController.Instance.masterMixer, "MusicVol", 30f, 0.4f);

        playerInput.enabled = false;

        // Blocca movimento e collisioni
        rb.linearVelocity = Vector2.zero;
        col.enabled = false;

        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.ResetCameraOffset();   
            movement.enabled = false;
        }

        // Particelle
        if (deathParticle != null)
        {
            Vector3 spawnPos = transform.position;
            spawnPos.z = riggedBody.transform.position.z;

            GameObject particles = Instantiate(deathParticle, spawnPos, Quaternion.identity);

            ApplyLayerFromPlayer(particles, gameObject);
            SetLayerRecursively(particles, GetCurrentPlayerLayer());
        }

        if (fullSprite != null)
        {
            var sr = fullSprite.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = false;
        }

        if (riggedBody != null)
        {
            riggedBody.transform.position = transform.position;
            riggedBody.transform.rotation = transform.rotation;
            riggedBody.transform.localScale = transform.localScale;

            riggedBody.SetActive(true);

            SetLayerRecursively(riggedBody, GetCurrentPlayerLayer());

            LightEffect_Shader.Instance.PlayEffect(deathColor, MaxIntensity, playerFadeInTime, playerFadeOutTime);
        }

        // Animazione morte
        if (animator != null)
            animator.SetTrigger("Die");

        // Aspetta animazione (qui il giocatore sente l'audio mentre vede l'animazione/particelle)
        yield return new WaitForSeconds(respawnDelay);

        // FADE OUT
        yield return SceneController.Instance.FadeOut(SceneController.Instance.fadeDuration);

        // Prepara il player (ancora invisibile)
        yield return StartCoroutine(PrepareRespawn());

        // Hold nero
        yield return new WaitForSeconds(blackScreenHoldTime);

        // Spegni rigged
        if (riggedBody != null)
            riggedBody.SetActive(false);

        // Reset mondo
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.ResetAll();
        if(WorldSwitch.Instance!=null && WorldSwitch.Instance.isFantasyWorldActive) 
            WorldSwitch.Instance.SwitchWorldWithoutAnimation();

        Slime_Blue_Big[] blueSlimes = FindObjectsByType<Slime_Blue_Big>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Slime_Blue_Big slime in blueSlimes)
        {
            slime.ResetEnemy();
        }

        Slime_Green_Big[] greenSlimes = FindObjectsByType<Slime_Green_Big>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Slime_Green_Big slime in greenSlimes)
        {
            slime.ResetEnemy();
        }

        StartCoroutine(SceneController.Instance.FadeIn(SceneController.Instance.fadeDuration));

        if (animator != null)
        {
            animator.SetTrigger("Respawn");
        }

        yield return StartCoroutine(PlayRespawnAnimation());

    }

    public IEnumerator PrepareRespawn()
    {
        if (fullSpriteRenderer == null)
            yield break;

        if (RespawnManager.Instance == null)
            yield break;

        Transform respawnPoint = RespawnManager.Instance.GetRespawnPoint();
        if (respawnPoint == null)
            yield break;

        if(WorldSwitch.Instance!=null)
            WorldSwitch.Instance.canSwitchWorld = false;

        // Imposto posizione
        transform.position = respawnPoint.position;

        //Setto layer e sorting in base al punto di respawn
        int targetLayer = GetCurrentPlayerLayer();
        string targetSortingLayer = GetCurrentSortingLayer();

        SetLayerRecursively(gameObject, targetLayer);

        if (fullSpriteRenderer != null)
        {
            ApplySortingRecursively(gameObject, targetSortingLayer, defaultSortingOrder);
        }

        //RESETTA LA GRAVITÀ DEL GIOCATORE
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.ResetStateForRespawn();
        }

        rb.simulated = false;
        rb.linearVelocity = Vector2.zero;

        transform.localScale = Vector3.one * spawnScale;

        fullSprite.SetActive(true);
        fullSpriteRenderer.enabled = true;

        Color c = fullSpriteRenderer.color;
        c.a = 0f;
        fullSpriteRenderer.color = c;

        yield return null;
    }

    private IEnumerator PlayRespawnAnimation()
    {
        if (fullSpriteRenderer == null) yield break;

        //Inizio a restettare la musica se non è un respawn di morte
        bool musicResetCalled = false;
        if (!isDying && !musicResetCalled)
        {
            StartCoroutine(AudioManager.Instance.ResetAudioOnPlayerSpawn());
            musicResetCalled = true;
        }

        if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.respawnSound);
                }

        yield return new WaitForSeconds(0.9f);

        // Colore iniziale invisibile
        Color c = fullSpriteRenderer.color;
        c.a = 0f;
        fullSpriteRenderer.color = c;

        // Scala iniziale
        transform.localScale = Vector3.one * spawnScale;

        // Fisica e input off
        rb.simulated = false;
        col.enabled = false;
        GetComponent<PlayerMovement>().enabled = false;

        float t = 0f;
        bool impulseGiven = false;
        bool shockWaveCalled = false;
        while (t < 1f)
        {
            t += Time.deltaTime / growDuration;
            t = Mathf.Clamp01(t);

            // Scala gradualmente
            transform.localScale = Vector3.Lerp(Vector3.one * spawnScale, Vector3.one, t);

            // Fade-in
            c.a = Mathf.Lerp(0f, 1f, t);
            fullSpriteRenderer.color = c;

            // Shockwave al 20% della chia>mata
            if (!shockWaveCalled && t >= 0.2f)
            {
                ShockWaveManager.Instance.CallShockWave(transform.position, shockWaveStrenght, shockWaveDuration);
                shockWaveCalled = true;
            }

            // Da impulso dopo un po'
            if (!impulseGiven && t >= 0.3f)
            {
                rb.simulated = true;
                rb.linearVelocity = Vector2.zero; // resetta velocità
                rb.AddForce(Vector2.up * upwardForce, ForceMode2D.Impulse);

                impulseGiven = true;
            }

            yield return null;
        }

        // riattivo collisioni e movimento
        col.enabled = true;
        GetComponent<PlayerMovement>().enabled = true;
        playerInput.enabled = true;

        if (!musicResetCalled) {
            //Rimetto la musica al volume normale chimanando un metodo che sta in AudioManager che resetta i parametri di transizione
            StartCoroutine(AudioManager.Instance.ResetAudioOnPlayerSpawn());
        }

        isDying = false;
    }

    //---------SPAWN NUOVA SCENA---------

    public void ForceSpawn(Transform point)
    {
        if (point == null) return;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        transform.position = point.position;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (fullSprite != null)
        {
            fullSprite.SetActive(true);
            if (fullSpriteRenderer != null)
                fullSpriteRenderer.enabled = true;
        }
    }

    public void TriggerSpawnAnimation()
    {
        if (fullSpriteRenderer == null) return;
        StartCoroutine(PlayRespawnAnimation());
    }

    public bool IsDying() { return isDying; }
     
    public void Die()
    {
        if (isDying) return;
        StartCoroutine(DeathSequence());
    }


    //Setta bene i layer al respawn
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private int GetCurrentPlayerLayer()
    {
        if (RespawnManager.Instance == null)
            return defaultLayer;

        Transform point = RespawnManager.Instance.GetRespawnPoint();
        if (point == null)
            return defaultLayer;

        if (point.gameObject.layer == LayerMask.NameToLayer(BACK_LAYER))
            return LayerMask.NameToLayer(PLAYER_BACK_LAYER);

        return LayerMask.NameToLayer(PLAYER_LAYER);
    }

    private string GetCurrentSortingLayer()
    {
        if (RespawnManager.Instance == null)
            return defaultSortingLayer;

        Transform point = RespawnManager.Instance.GetRespawnPoint();
        if (point == null)
            return defaultSortingLayer;

        if (point.gameObject.layer == LayerMask.NameToLayer(BACK_LAYER))
            return PLAYER_BACK_SORTING;

        return PLAYER_SORTING;
    }

    private void ApplyLayerFromPlayer(GameObject target, GameObject player)
    {
        int layer = player.layer;

        // Layer
        SetLayerRecursively(target, layer);

        // Sorting Layer (usa quello del player visivo)
        if (fullSpriteRenderer != null)
        {
            string sortingLayer = fullSpriteRenderer.sortingLayerName;
            int order = fullSpriteRenderer.sortingOrder;

            SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in renderers)
            {
                sr.sortingLayerName = sortingLayer;
                sr.sortingOrder = order;
            }

            ParticleSystemRenderer[] particles = target.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (var p in particles)
            {
                p.sortingLayerName = sortingLayer;
                p.sortingOrder = order;
            }
        }
    }

    private void ApplySortingRecursively(GameObject obj, string sortingLayer, int order)
    {
        //Sorting Group (priorità massima)
        SortingGroup[] groups = obj.GetComponentsInChildren<SortingGroup>(true);
        foreach (var sg in groups)
        {
            sg.sortingLayerName = sortingLayer;
            sg.sortingOrder = order;
        }

        // Sprite Renderer
        SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in renderers)
        {
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = order;
        }

        // Particle System
        ParticleSystemRenderer[] particles = obj.GetComponentsInChildren<ParticleSystemRenderer>(true);
        foreach (var p in particles)
        {
            p.sortingLayerName = sortingLayer;
            p.sortingOrder = order;
        }
    }
}