using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected Animator animator;
    protected SpriteRenderer sr;
    protected Rigidbody2D rb;
    protected Collider2D[] allColliders;
    protected EnemyHead[] heads;

    public float patrolSpeed = 3.0f;
    public bool isKillable = true;

    protected bool isDead = false;
    protected bool isActive = false;
    protected bool isVisible = false; // se è entrato nella camera almeno una volta
    protected bool canMove = true;
    protected int direction = 1;       // 1 = destra, -1 = sinistra

    private float lastFlipTime;
    [SerializeField] private float flipCooldown = 0.2f;

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
        if (isDead || !isActive || !canMove) return;
        Move();
    }

    // Ogni nemico deve implementare il suo movimento
    protected abstract void Move();

    // Ogni nemico deve implementare il suo suono
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

        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Enemy"))
        {
            OnObstacleHit();
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
            OnObstacleHit();
        }
    }

    public virtual void OnStomped()
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

    protected void Flip()
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
        if (allColliders != null)
        {
            foreach (var c in allColliders)
            {
                if (c != null) 
                    c.enabled = true;
            }
        }

        // Reset delle EnemyHead
        if (heads != null)
        {
            foreach (var head in heads)
            {
                if (head != null) 
                    head.ResetHead();
            }
        }

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

    protected virtual void OnObstacleHit()
    {
        Flip();
    }

    public void SetDirection(int dir)
    {
        direction = dir;

        if (sr != null)
            sr.flipX = direction < 0;
    }

    // Permette di bloccare o sbloccare il movimento del nemico (usato per i piccoli slime che spawnano)
    public void SetMovement(bool value)
    {
        canMove = value;
    }
}