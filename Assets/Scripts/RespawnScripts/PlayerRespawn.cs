using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerRespawn : MonoBehaviour
{
    private PlayerInput playerInput;

    public ScreenFade screenFade;
    public SceneController sceneController;

    private Transform respawnRune;

    [Header("Magic Respawn")]
    public float spawnScale = 0.2f;
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


    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        playerInput = GetComponent<PlayerInput>();
        if (fullSprite != null)
            fullSpriteRenderer = fullSprite.GetComponent<SpriteRenderer>();

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
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(AudioManager.instance.playerDeathSound);
        }

        playerInput.enabled = false;

        // Blocca movimento e collisioni
        rb.linearVelocity = Vector2.zero;
        col.enabled = false;

        var movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

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

        // FADE IN
        yield return screenFade.FadeInCoroutine(sceneController.fadeDuration);

        // Animazione uscita dalla roccia
        yield return StartCoroutine(PlayRespawnAnimation());

        // Input leggermente dopo
        yield return new WaitForSeconds(sceneController.fadeDuration * 0.2f);
        playerInput.enabled = true;

        isDying = false;

    }

    private IEnumerator PrepareRespawn()
    {
        if (fullSpriteRenderer == null || respawnRune == null)
            yield break;

        transform.position = respawnRune.position;

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
        Color c = fullSpriteRenderer.color;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / growDuration;

            transform.localScale = Vector3.Lerp(
                Vector3.one * spawnScale,
                Vector3.one,
                t
            );

            c.a = Mathf.Lerp(0f, 1f, t);
            fullSpriteRenderer.color = c;

            yield return null;
        }

        rb.simulated = true;
        rb.AddForce(Vector2.up * upwardForce, ForceMode2D.Impulse);

        col.enabled = true;
        GetComponent<PlayerMovement>().enabled = true;
    }

    public void SetRespawnPoint(Transform rune)
    {
        respawnRune = rune;
    }
    public bool IsDying() { return isDying; }
     
    public void Die()
    {
        if (isDying) return;
        StartCoroutine(DeathSequence());
    }
}