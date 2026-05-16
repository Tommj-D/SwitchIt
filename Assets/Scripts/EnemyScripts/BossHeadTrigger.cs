using UnityEngine;

public class BossHeadTrigger : MonoBehaviour
{
    [Tooltip("Trascina qui il corpo principale del boss che contiene il BossManager")]
    public BossManager bossManager;
    
    [Tooltip("La forza con cui il player rimbalza dopo aver colpito la testa")]
    public float bounceForce = 15f; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Controlla se chi è atterrato sulla testa è il Player
        if (collision.CompareTag("Player"))
        {
            // 1. Diamo il colpo al boss
            if (bossManager != null)
            {
                bossManager.PrendiDanno();
            }

            // 2. Facciamo rimbalzare il giocatore in alto!
            Rigidbody2D rbPlayer = collision.GetComponent<Rigidbody2D>();
            if (rbPlayer != null)
            {
                // Resetta la velocità verticale per evitare un salto troppo debole o troppo forte
                rbPlayer.linearVelocity = new Vector2(rbPlayer.linearVelocity.x, 0f);
                
                // Da la spinta
                rbPlayer.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
            }
        }
    }
}