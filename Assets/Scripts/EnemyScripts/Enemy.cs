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

    protected SpriteRenderer sr;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
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
        if (isDead) return;
        if (WorldSwitch.Instance != null && WorldSwitch.Instance.isSwitching) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            var respawn = collision.gameObject.GetComponent<PlayerRespawn>();
            if (respawn != null)
                respawn.Die();
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            // se la collisione è laterale
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                Flip();
                break;
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // Se il nemico cade in un buco o tocca un'area di morte
        if (collision.gameObject.CompareTag("Death"))
        {
            OnStomped();
        }
        
        // Se tocca un muro o un ostacolo, cambia direzione
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Flip();
        }
    }

    public void OnStomped()
    {
        if (isDead) return;

        Debug.Log("OnStomped: " + gameObject.name, gameObject);
        
        isDead = true;
        isActive = false;

        // Suono morte
        Sound();

        // Disabilita tutti i collider del nemico (corpo + figli)
        var allColliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in allColliders)
            col.enabled = false;

        // Imposta Rigidbody kinematic e zero velocity
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        // Animazione morte
        if (animator != null)
            animator.SetTrigger("Die");
    }

    private void Flip()
    {
        //if (WorldSwitch.Instance!=null && WorldSwitch.Instance.isSwitching) return;
        direction *= -1;

        if (sr != null)
            sr.flipX = direction < 0;
    }

    private void OnBecameVisible()
    {
        if (!isDead)
        {
            isActive = true;
            isVisible = true;
        }
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

        // Reset delle EnemyHead
        var heads = GetComponentsInChildren<EnemyHead>();
        foreach (var head in heads)
            head.ResetHead();

        var renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.isVisible)
        {
            isActive = true;
            isVisible = true;
        }
    }

    public void DisableEnemy()
    {
        gameObject.SetActive(false);
    }
}