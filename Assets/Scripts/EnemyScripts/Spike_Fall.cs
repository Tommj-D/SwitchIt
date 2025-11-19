using UnityEngine;

public class Spike_Fall : MonoBehaviour
{
    public float fallSpeed = 3f;
    public float shakeDuration = 0.3f;
    public float shakeAmount = 0.1f;

    public GameObject impactParticles;

    private bool isShaking = false;
    private bool isFalling = false;

    private Vector3 originalPosition;
    private Rigidbody2D rb;

    void Start()
    {
        originalPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
    }

    void FixedUpdate()
    {
        // Tremolio
        if (isShaking)
        {
            Vector3 shakePos = originalPosition + (Vector3)Random.insideUnitCircle * shakeAmount;
            rb.MovePosition(shakePos);
        }

        // Caduta
        if (isFalling)
        {
            Vector2 newPos = rb.position + Vector2.down * fallSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(StartShake());
        }
    }

    private System.Collections.IEnumerator StartShake()
    {
        isShaking = true;

        yield return new WaitForSeconds(shakeDuration);

        // Fine tremolio
        isShaking = false;
        rb.MovePosition(originalPosition);

        // Inizia la caduta
        isFalling = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Controlla se ha colpito il terreno e fa partire particelle 
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (impactParticles != null)
                Instantiate(impactParticles, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
