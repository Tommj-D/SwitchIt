using UnityEngine;

public class AirBoost : MonoBehaviour
{
    public float forzaSpinta = 15f;
    public float cooldown = 0.2f;

    [Header("Direzione spinta")]
    public Vector2 direzione = Vector2.up;

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

        Vector2 dir = direzione.normalized;

        if (dir.y != 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        if (dir.x != 0)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        rb.AddForce(dir * forzaSpinta, ForceMode2D.Impulse);
        timer = cooldown;
    }
}
