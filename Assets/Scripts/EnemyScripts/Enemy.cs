using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected Animator animator;

    public AudioClip dieSound;
    public float patrolSpeed = 1.0f;
    public bool isKillable = true;


    protected bool isDead = false;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        if (isDead) return;
        Move();
    }

    // OGNI nemico deve implementare il suo movimento
    protected abstract void Move();

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var respawn = collision.gameObject.GetComponent<PlayerRespawn>();
            if (respawn != null)
                respawn.Die();
        }
    }

    public virtual void OnStomped(GameObject player)
    {
        if (!isKillable) return;

        isDead = true;

        animator.SetTrigger("Die");

        if (dieSound)
            AudioSource.PlayClipAtPoint(dieSound, transform.position);

        // Disabilita fisica
        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;    // Disabilita fisica
        }

        // rimbalzare il player
        var playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 8f);


        Destroy(gameObject, 3f);
    }
}
