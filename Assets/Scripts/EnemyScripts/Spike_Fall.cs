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

    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void OnEnable()
    {
        ResetSpike();
    }

    private void Update()
    {
        if (isShaking)
        {
            Vector2 rand = Random.insideUnitCircle * shakeAmount;
            transform.position = startPosition + new Vector3(rand.x, rand.y, 0f);
        }

        if (isFalling)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        }
    }

    public void ResetSpike()
    {
        StopAllCoroutines();
        isFalling = false;
        isShaking = false;
        activated = false;
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

        if (AudioManager.Instance.shakeSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.shakeSound);
        }
        yield return new WaitForSeconds(shakeDuration);

        isShaking = false;
        transform.position = startPosition;

        isFalling = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Se tocca un oggetto con layer "Player"
        if (collision.gameObject.CompareTag("Player"))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.spikeCrashSound);
            }
            // Istanzia le particelle
            if (impactParticles != null)
            {
                Instantiate(impactParticles, transform.position, Quaternion.identity);
            }

            // Distruggi la spina
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        // Se la spina è già partita in caduta o shaking, distruggila
        if (activated || isFalling || isShaking)
        {
            gameObject.SetActive(false);
        }
    }
}
