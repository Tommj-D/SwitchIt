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

        if (collision.transform.position.y < transform.position.y)
            return;
            
        // Controllo che il player stia cadendo
        if (rb.linearVelocity.y >= 0)
            return;

        PlayerRespawn playerRespawn = collision.GetComponent<PlayerRespawn>();
        if (playerRespawn != null && playerRespawn.IsDying())
            return;

        hasBeenStomped = true;

        // Rimbalzo 
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingForce);

        PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.ResetJumps();

        // Ora uccido il nemico
        enemy.OnStomped();

        // Score
        ScoreManager.instance?.SegnalaNemicoSconfitto();

        // Disattivo collider testa
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