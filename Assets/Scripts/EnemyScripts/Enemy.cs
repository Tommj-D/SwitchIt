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
    private float lastFlipTime;
    [SerializeField] private float flipCooldown = 0.2f;
    protected SpriteRenderer sr;
    protected Rigidbody2D rb;
    protected Collider2D[] allColliders;
    protected EnemyHead[] heads;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        allColliders = GetComponents<Collider2D>();
        heads = GetComponentsInChildren<EnemyHead>();

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

        isDead = true;
        isActive = false;

        Sound();

        foreach (var col in allColliders)
            col.enabled = false;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        animator?.SetTrigger("Die");
    }

    private void Flip()
    {
        if (Time.time - lastFlipTime < flipCooldown) return;

        lastFlipTime = Time.time;

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
        if (rb)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;
        }

        // Riattiva collider
        foreach (var c in allColliders)
            c.enabled = true;

        // Reset delle EnemyHead
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