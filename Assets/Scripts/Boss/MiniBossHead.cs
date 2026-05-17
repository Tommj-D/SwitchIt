using UnityEngine;

public class MiniBossHead : MonoBehaviour
{
    private MiniSpawnBoss boss;

    private void Start()
    {
        boss = GetComponentInParent<MiniSpawnBoss>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        boss.Die();

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        PlayerMovement player = collision.GetComponent<PlayerMovement>();

        // 👉 bounce player
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * 7f, ForceMode2D.Impulse);
        }

        // 👉 reset salti
        if (player != null)
        {
            player.ResetJumps();
        }
    }
}