using UnityEngine;

public class EnemyHead : MonoBehaviour
{
    public Enemy enemy;

    public float jumpingForce = 8f;

    private bool hasBeenStomped = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasBeenStomped) return;

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();

        // ===== CASO PLAYER =====
        if (playerMovement != null)
        {
            float gravitySign = playerMovement.IsGravityInverted() ? -1f : 1f;

            if ((collision.transform.position.y - transform.position.y) * gravitySign < 0)
                return;

            if (rb.linearVelocity.y * gravitySign >= 0)
                return;

            PlayerRespawn playerRespawn = collision.GetComponent<PlayerRespawn>();
            if (playerRespawn != null && playerRespawn.IsDying())
                return;

            // Rimbalzo player
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpingForce * gravitySign, ForceMode2D.Impulse);

            playerMovement.ResetJumps();
        }

        // ===== CASO ENEMY =====
        else if (collision.GetComponent<Enemy>() != null)
        {
            // Qui puoi decidere se mettere controlli o no
            // Per ora: attiva sempre
        }
        else
        {
            return;
        }

        // ===== COMUNE =====
        hasBeenStomped = true;

        enemy.OnStomped();

        ScoreManager.instance?.SegnalaNemicoSconfitto();

        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;
    }

    public void ResetHead()
    {
        hasBeenStomped = false;

        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = true;
    }
}