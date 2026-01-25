using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerRespawn : MonoBehaviour
{
    private PlayerInput playerInput;

    public ScreenFade screenFade;
    public SceneController sceneController;

    public Vector3 respawnPoint;
    [Header("Respawn Timing")]
    public float blackScreenHoldTime = 0.2f;
    public float respawnDelay = 1.5f;
    
    public GameObject deathParticle;
    public GameObject fullSprite;      
    public GameObject riggedBody;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;

    private bool isDying = false;


    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        playerInput = GetComponent<PlayerInput>();
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

        // FADE OUT (schermo si scurisce) 
        yield return screenFade.FadeOutCoroutine(sceneController.fadeDuration);

        // Prima del respawn, resetta oggetti
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.ResetAll();

        // Respawn
        transform.position = respawnPoint;
        
        if (riggedBody != null) riggedBody.SetActive(false);

        if (fullSprite != null)
        {
            var sr = fullSprite.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;
        }

        yield return new WaitForSeconds(blackScreenHoldTime);

        if (animator != null)
            animator.SetTrigger("Respawn");

        if (movement != null) movement.enabled = true;
        if (col != null) col.enabled = true;


        yield return null;

        // FADE IN (torna visibile)
        Coroutine fadeIn = StartCoroutine(
            screenFade.FadeInCoroutine(sceneController.fadeDuration)
        );

        // In modo che i comandi si attivino quando il gioco torna visibile
        yield return new WaitForSeconds(sceneController.fadeDuration * 0.2f);


        playerInput.enabled = true;
        isDying = false;

        yield return fadeIn;
    }

    public bool IsDying() { return isDying; }
     
    public void Die()
    {
        if (isDying) return;
        StartCoroutine(DeathSequence());
    }
}