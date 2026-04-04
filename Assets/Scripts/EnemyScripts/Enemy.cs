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

    [Header("Split Settings")]
    public bool canSplit = false;
    public int size = 1;
    public GameObject smallerVersionPrefab;
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

        if(!collision.gameObject.CompareTag("Ground"))
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
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic; 
            }
        }

        // Se può splittare, NON distruggere subito
        if (canSplit && size > 1)
        {
            animator?.SetTrigger("Split"); // animazione diversa
        }
        else
        {
            animator?.SetTrigger("Die");
        }
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

    //Funzione che chiamo se il nemico è grande e volgio che si divida in 2 più piccoli
    protected virtual void Split()
    {
        float forceX = 4f;
        float forceY = 3f;

        for (int i = 0; i < 2; i++)
        {
            GameObject newEnemy = Instantiate(smallerVersionPrefab, transform.position, Quaternion.identity);

            Enemy enemyScript = newEnemy.GetComponent<Enemy>();
            enemyScript.size = size - 1;

            Rigidbody2D newRb = newEnemy.GetComponent<Rigidbody2D>();
            if (newRb != null)
            {
                float dir = (i == 0) ? -1 : 1;

                // reset velocità per sicurezza
                newRb.linearVelocity = Vector2.zero;

                // spinta più “bella”
                newRb.AddForce(new Vector2(dir * forceX, forceY), ForceMode2D.Impulse);
            }
        }

        Destroy(gameObject);
    }

    public void DisableEnemy()
    {
        gameObject.SetActive(false);
    }

    protected virtual void OnObstacleHit()
    {
        Flip();
    }

    public void PerformSplit()
    {
        Split();
    }
}