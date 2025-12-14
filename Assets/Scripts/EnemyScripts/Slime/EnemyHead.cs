using UnityEngine;

public class EnemyHead : MonoBehaviour
{
    public Enemy enemy;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Uccidi il nemico (il parent)
            enemy.OnStomped(collision.gameObject);

            // Rimbalzo del player
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 8f);
        }
    }
}
