using UnityEngine;

public class Slime_Green : MonoBehaviour
{
    private Animator animator;

    public float patrolSpeed = 1.0f;
    public bool patrolling = true;
    public AudioClip dieSound;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
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
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            // Controllo contatti per capire se il player ha colpito dall'alto
            bool stomped = false;
            foreach (var contact in collision.contacts)
            {
                // Un valore normal.y > 0.5 indica che il contatto arriva dall'alto (player sopra)
                if (contact.normal.y > 0.5f)
                {
                    stomped = true;
                    break;
                }
            }

            if (!stomped && playerRb != null)
            {
                if (playerRb.linearVelocity.y < -0.5f && collision.transform.position.y > transform.position.y + 0.2f)
                    stomped = true;
            }

            if (stomped)
            {
                OnStomped(collision.gameObject);
            }
            else
            {
                // morire
                var respawn = collision.gameObject.GetComponent<PlayerRespawn>();
                if (respawn != null)
                {
                    respawn.Die();
                }
            }
        }
    }

    private void OnStomped(GameObject player)
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
    }
}

