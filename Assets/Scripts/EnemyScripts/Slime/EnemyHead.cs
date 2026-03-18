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

        // Deve avere rigidbody ed essere in caduta
        if (rb == null)
            return;

        PlayerRespawn playerRespawn = collision.GetComponent<PlayerRespawn>();
        if (playerRespawn != null && playerRespawn.IsDying())
            return;

        hasBeenStomped = true;

        // Uccido il nemico
        enemy.OnStomped();

        if (ScoreManager.instance != null)
            ScoreManager.instance.SegnalaNemicoSconfitto();

        // Rimbalzo player
        if (rb != null)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingForce);

        PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.ResetJumps();

        // Disattivo i collider della testa
        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }
    }

    //Chiamata da enemy resetta la testa del nemico, in modo che possa essere nuovamente calpestata se il nemico respawna
    public void ResetHead()
    {
        hasBeenStomped = false;

        // Riattiva i collider della testa
        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = true;
    }
}
