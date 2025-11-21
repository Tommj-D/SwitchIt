using UnityEngine;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    public ScreenFade screenFade;

    public Vector3 respawnPoint;
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
        isDying = true;

        // Blocca movimento e collisioni
        rb.linearVelocity = Vector2.zero;
        col.enabled = false;

        var movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        // Particelle
        if (deathParticle != null)
            Instantiate(deathParticle, transform.position, Quaternion.identity);

        if (fullSprite != null)
        {
            var sr = fullSprite.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = false;
        }

        if (riggedBody != null)
        {
            // Mantieni pos/rot/scale del player sul rig per evitare "salti"
            riggedBody.transform.position = transform.position;
            riggedBody.transform.rotation = transform.rotation;
            riggedBody.transform.localScale = transform.localScale;

            riggedBody.SetActive(true);
        }

        // Animazione morte
        if (animator != null)
            animator.SetTrigger("Die");

        // Aspetta animazione
        yield return new WaitForSeconds(respawnDelay);

        // Respawn
        transform.position = respawnPoint;

        if (riggedBody != null) riggedBody.SetActive(false);
        if (fullSprite != null)
        {
            var sr = fullSprite.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;
        }

        // Rianima
        if (animator != null)
             animator.SetTrigger("Respawn");

        if (movement != null) movement.enabled = true;

        // Riattiva fisica e collisioni
        col.enabled = true;

        isDying = false;
    }

    public bool IsDying() { return isDying; }
}
