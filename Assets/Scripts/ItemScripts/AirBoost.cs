using UnityEngine;

public class AirBoost : MonoBehaviour
{
    public float forzaSpinta = 15f;
    public float cooldown = 0.2f;

    private float timer;

    private void Update()
    {
        if (timer > 0)
            timer -= Time.deltaTime;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (timer > 0) return;

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // Se il player sta scendendo o è fermo
        if (rb.linearVelocity.y <= 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * forzaSpinta, ForceMode2D.Impulse);
            timer = cooldown;
        }
    }
}
