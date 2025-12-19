using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected Animator animator;

    public AudioClip dieSound;
    public float patrolSpeed = 3.0f;
    public bool isKillable = true;

    protected bool isDead = false;
    protected bool isActive = false;

    protected int direction = 1;           // 1 = destra, -1 = sinistra

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        if (isDead || !isActive) return;
        Move();
    }

    // OGNI nemico deve implementare il suo movimento
    protected abstract void Move();

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")&&!isDead)
        {
            var respawn = collision.gameObject.GetComponent<PlayerRespawn>();
            if (respawn != null)
                respawn.Die();
        }
       Flip();
    }

    //Per non far cadere il nemico dai bordi delle piattaforme
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Death"))
        {
            Death();
        }
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Flip();
        }
    }

    public virtual void OnStomped(GameObject player)
    {
        if (!isKillable) return;
        
        Death();

        // rimbalzare il player
        var playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 8f);


        Invoke(nameof(DisableEnemy), 3f);
    }

    private void Death()
    {
        isDead = true;

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        animator.SetTrigger("Die");

        if (dieSound)
            AudioSource.PlayClipAtPoint(dieSound, transform.position);

        // Disabilita fisica
        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;    // Disabilita fisica
        }
    }
    private void Flip()
    {
        direction *= -1; // cambia direzione
        Vector3 scale = transform.localScale;
        scale.x *= -1;   // capovolge lo sprite
        transform.localScale = scale;
    }

    //In modo da attivare il nemico solo quando entra nella camera
    private void OnBecameVisible()
    {
        isActive = true; // attiva il nemico quando entra nella camera
    }

   /*private void OnBecameInvisible()
    {
        isActive = false;
    }*/

    private void DisableEnemy()
    {
        gameObject.SetActive(false);
    }
}
