using UnityEngine;

public class Slime_Green : Enemy
{
    protected override void Move()
    {
        transform.Translate(Vector2.right * patrolSpeed * Time.deltaTime);
    }
    /*private Animator animator;

    public float patrolSpeed = 1.0f;
    public bool patrolling = true;
    public AudioClip dieSound;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (patrolling)
        {
            // semplice pattugliamento orizzontale (adatta come vuoi)
            transform.Translate(Vector2.right * patrolSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var respawn = collision.gameObject.GetComponent<PlayerRespawn>();
            respawn.Die();
        }
    }

    public void OnStomped(GameObject player)
    {
        animator.SetTrigger("Die");

        // play sound / vfx
        if (dieSound) AudioSource.PlayClipAtPoint(dieSound, transform.position);

        // Optional: disabilita collider e sprite per effetto "schiacciato"
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // fai rimbalzare il player
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 8f); // regola la forza del rimbalzo
        }

        // distruggi il nemico dopo breve delay
        Destroy(gameObject, 4f);
    }*/
}

