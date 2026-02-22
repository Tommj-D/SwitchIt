using UnityEngine;

public class EnemyHead : MonoBehaviour
{
    public Enemy enemy;

    public float jumpingForce = 8f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            enemy.OnStomped(collision.gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerRespawn playerRespawn = collision.GetComponent<PlayerRespawn>();
        if (playerRespawn == null || playerRespawn.IsDying())
            return;

        enemy.OnStomped(collision.gameObject);

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingForce);

        PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.ResetJumps();

        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }
    }
}
