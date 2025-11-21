using UnityEngine;
using System.Collections;

public class Spike_Fall : MonoBehaviour
{
    public float fallSpeed = 3f;
    public float shakeDuration = 0.3f;
    public float shakeAmount = 0.1f;
    public GameObject impactParticles;   

    private bool isFalling = false;
    private bool isShaking = false;
    private bool activated = false; // impedisce ri-attivazioni

    private Vector3 originalPosition;

    void Awake()
    {
        // salva la posizione iniziale qui 
        originalPosition = transform.position;
    }

    void OnEnable()
    {
        // Quando l'oggetto viene riattivato, resetta stato e posizione
        isFalling = false;
        isShaking = false;
        activated = false;
        transform.position = originalPosition;
    }

    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        // Tremolio
        if (isShaking)
        {
            Vector2 rand = Random.insideUnitCircle * shakeAmount;
            transform.position = originalPosition + new Vector3(rand.x, rand.y, 0f);
        }

        // Caduta
        if (isFalling)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !activated)
        {
            activated = true;
            StartCoroutine(StartShake());
        }
    }

    private IEnumerator StartShake()
    {
        isShaking = true;

        yield return new WaitForSeconds(shakeDuration);

        isShaking = false;
        transform.position = originalPosition;

        isFalling = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Se tocca un oggetto con layer "Ground"
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")||collision.gameObject.CompareTag("Player"))
        {
            // Istanzia le particelle
            if (impactParticles != null)
            {
                Instantiate(impactParticles, transform.position, Quaternion.identity);
            }

            // Distruggi la spina
            gameObject.SetActive(false);
        }
    }
}
