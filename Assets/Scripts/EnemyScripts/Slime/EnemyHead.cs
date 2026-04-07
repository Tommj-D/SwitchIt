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
        if (playerMovement == null) return;

        // Direzione gravità reale del player
        float gravitySign = playerMovement.IsGravityInverted() ? -1f : 1f;

        // Controllo posizione (devo essere "sopra" rispetto alla gravità)
        if ((collision.transform.position.y - transform.position.y) * gravitySign < 0)
            return;

        // Controllo movimento (devo andare verso il nemico)
        if (rb.linearVelocity.y * gravitySign >= 0)
            return;

        PlayerRespawn playerRespawn = collision.GetComponent<PlayerRespawn>();
        if (playerRespawn != null && playerRespawn.IsDying())
            return;

        hasBeenStomped = true;

        // Rimbalzo
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpingForce * gravitySign, ForceMode2D.Impulse);

        playerMovement.ResetJumps();

        // Uccido nemico
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