using UnityEngine;

public class MiniBossHead : MonoBehaviour
{
    private MiniSpawnBoss miniBoss;

    private void Start()
    {
        // Trova lo script principale del boss sul genitore
        miniBoss = GetComponentInParent<MiniSpawnBoss>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy")) miniBoss.Die();

        // Se non è il giocatore, non fare nulla
        if (!collision.CompareTag("Player")) return;
        
        // ==================================================
        // SICUREZZA ANTI-MORTE CONTEMPORANEA
        // ==================================================
        if (miniBoss != null)
        {
            // Troviamo TUTTI i collider presenti sul minion (corpo e testa)
            Collider2D[] allColliders = miniBoss.GetComponentsInChildren<Collider2D>();
            
            // Li disattiviamo immediatamente. Diventando intangibile, 
            // il minion non potrà più attivare lo script di danno sul giocatore.
            foreach (Collider2D col in allColliders)
            {
                col.enabled = false;
            }

            // Ordiniamo al boss di morire
            miniBoss.Die();
        }

        // ==================================================
        // GESTIONE RIMBALZO GIOCATORE
        // ==================================================
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        PlayerMovement player = collision.GetComponent<PlayerMovement>();

        // bounce player (spinta verso l'alto)
        if (rb != null)
        {
            // Azzeriamo la velocità verticale attuale per avere sempre un rimbalzo costante
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * 7f, ForceMode2D.Impulse);
        }

        // reset salti
        if (player != null)
        {
            player.ResetJumps();
        }
    }
}