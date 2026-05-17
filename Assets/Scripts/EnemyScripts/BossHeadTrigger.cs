using UnityEngine;

public class BossHeadTrigger : MonoBehaviour
{
    [Tooltip("Trascina qui il corpo principale del boss che contiene il BossManager")]
    public BossManager bossManager;

    [Tooltip("La forza con cui il player rimbalza dopo aver colpito la testa")]
    public float bounceForce = 15f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // 1. Boss prende danno
        if (bossManager != null)
        {
            bossManager.PrendiDanno();
        }

        // 2. Player bounce
        Rigidbody2D rbPlayer = collision.GetComponent<Rigidbody2D>();
        if (rbPlayer != null)
        {
            rbPlayer.linearVelocity = new Vector2(rbPlayer.linearVelocity.x, 0f);
            rbPlayer.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
        }

        // 3. RESET SALTI (QUI MANCAVA)
        PlayerMovement player = collision.GetComponent<PlayerMovement>();
        if (player != null)
        {
            player.ResetJumps();
        }
    }
}