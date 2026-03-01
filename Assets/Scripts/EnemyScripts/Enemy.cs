using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected Animator animator;

    public float patrolSpeed = 3.0f;
    public bool isKillable = true;

    protected bool isDead = false;
    protected bool isActive = false;
    protected bool isVisible = false; // se è entrato nella camera almeno una volta

    protected int direction = 1; // 1 = destra, -1 = sinistra

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        if (isDead || !isActive) return;

        Move();
    }

    // Ogni nemico deve implementare il suo movimento (es. in uno script che eredita da questo)
    protected abstract void Move();

    // Ogni nemico deve implementare il suo suono (es. in uno script che eredita da questo)
    protected abstract void Sound();

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isDead)
        {
            var respawn = collision.gameObject.GetComponent<PlayerRespawn>();
            if (respawn != null)
                respawn.Die();
        }
        Flip();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // Se il nemico cade in un buco o tocca un'area di morte
        if (collision.gameObject.CompareTag("Death"))
        {
            Death();
        }
        
        // Se tocca un muro o un ostacolo, cambia direzione
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Flip();
        }
    }

    public virtual void OnStomped(GameObject player)
    {
        if (!isKillable || isDead) return;
        
        Death();

        // Fa rimbalzare il giocatore verso l'alto
        var playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 8f);

        // Disabilita definitivamente l'oggetto dopo 3 secondi
        Invoke(nameof(DisableEnemy), 3f);
    }

    private void Death()
    {
        if (isDead) return; // Evita di chiamare la morte più volte
        isDead = true;

        // Suono morte
        Sound();

        // Disabilita tutti i collider per evitare altre collisioni
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // Avvia l'animazione di morte
        if (animator != null)
            animator.SetTrigger("Die");

        // Disabilita la fisica per farlo "scivolare" o restare fermo
        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Flip()
    {
        direction *= -1; 
        Vector3 scale = transform.localScale;
        scale.x *= -1;   
        transform.localScale = scale;
    }

    private void OnBecameVisible()
    {
        if (!isDead)
        {
            isActive = true;
            isVisible = true;
        }
    }

    private void DisableEnemy()
    {
        gameObject.SetActive(false);
    }

    public virtual void ResetEnemy()
    {
        isDead = false;
        isKillable = true;

        isActive = false;
        isVisible = false;

        // Riattiva Rigidbody
        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;
        }

        // Riattiva collider
        var colliders = GetComponentsInChildren<Collider2D>();
        foreach (var c in colliders)
            c.enabled = true;

        var renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.isVisible)
        {
            isActive = true;
            isVisible = true;
        }
    }
}