using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    private PlayerInput playerInput;

    public ScreenFade screenFade;
    public SceneController sceneController;

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
  
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer fullSpriteRenderer;

    private bool isDying = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        playerInput = GetComponent<PlayerInput>();

        if (fullSprite != null)
            fullSpriteRenderer = fullSprite.GetComponent<SpriteRenderer>();
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
        if (!isDying && collision.gameObject.CompareTag("Enemy"))
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
            GameObject particles = Instantiate(deathParticle, transform.position, Quaternion.identity);
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
        }

        // Animazione morte
        if (animator != null)
            animator.SetTrigger("Die");

        // Aspetta animazione (qui il giocatore sente l'audio mentre vede l'animazione/particelle)
        yield return new WaitForSeconds(respawnDelay);

        // FADE OUT
        yield return screenFade.FadeOutCoroutine(sceneController.fadeDuration);

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

        if (animator != null)
        {
            animator.SetTrigger("Respawn");
        }

        StartCoroutine(PlayRespawnAnimation());

        playerInput.enabled = true;

        // FADE IN
        yield return screenFade.FadeInCoroutine(sceneController.fadeDuration);

        isDying = false;

    }

    private IEnumerator PrepareRespawn()
    {
        if (fullSpriteRenderer == null)
            yield break;

        if (RespawnManager.Instance == null)
            yield break;

        Transform respawnPoint = RespawnManager.Instance.GetRespawnPoint();
        if (respawnPoint == null)
            yield break;

        // Imposto posizione
        transform.position = respawnPoint.position;

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


        VolumeController.Instance.FadeMixerParam(VolumeController.Instance.transitionMixer, "MusicPitch", 1f, 1.3f);

        float t = 0f;
        bool impulseGiven = false;
        while (t < 1f)
        {
            t += Time.deltaTime / growDuration;
            t = Mathf.Clamp01(t);

            // Scala gradualmente
            transform.localScale = Vector3.Lerp(Vector3.one * spawnScale, Vector3.one, t);

            // Fade-in
            c.a = Mathf.Lerp(0f, 1f, t);
            fullSpriteRenderer.color = c;

            // Dai impulso dopo un po'
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

        //Rimetto la musica al volume normale
        AudioTransitionManager transition = AudioManager.Instance.GetComponent<AudioTransitionManager>();
        VolumeController.Instance.FadeMixerParam(VolumeController.Instance.masterMixer, "MusicVol", 0f, 1.4f);
        VolumeController.Instance.FadeMixerParam(VolumeController.Instance.transitionMixer, "MusicLowpass", 22000f, 1.1f);
        VolumeController.Instance.FadeMixerParam(VolumeController.Instance.transitionMixer, "MusicHightpass", 10f, 1.1f);
        VolumeController.Instance.FadeMixerParam(VolumeController.Instance.transitionMixer, "SFXLowpass", 22000f, 1.1f);

        yield return new WaitForSeconds(1.4f);
        transition.ExitTransition();
        VolumeController.Instance.ResetMusicState(0.2f);
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
}