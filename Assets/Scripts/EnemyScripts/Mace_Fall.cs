using UnityEngine;
using System.Collections;

public class Mace_Fall : MonoBehaviour
{
    public float fallSpeed = 8f;
    public float upSpeed = 2f;
    public float shakeDuration = 0.5f;
    public float shakeAmount = 0.1f;
    public GameObject impactParticles;

    public LayerMask playerLayer;

    private bool isFalling = false;
    private bool isShaking = false;
    private bool isActivated = false;

    private Vector3 originalPosition;
    private Collider2D myCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = transform.position;
        myCollider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
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
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;
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

    private IEnumerator StartGoUp()
    {
        yield return new WaitForSeconds(0.5f);
        float targetY = originalPosition.y;
        while (transform.position.y < targetY)
        {
            transform.Translate(Vector3.up * upSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = originalPosition;
        isActivated = false; // Reset activation for future triggers

        if (IsPlayerUnder())
        {
            // Riattiva la sequenza 
            isActivated = true;
            StartCoroutine(StartShake());
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (isFalling && other.gameObject.CompareTag("Ground"))
        {
            isFalling = false;
            if (impactParticles != null)
            {
                Instantiate(impactParticles, transform.position + new Vector3(0, -0.65f, 0), Quaternion.identity);
            }
            StartCoroutine(StartGoUp());
        }
    }

    private bool IsPlayerUnder()
    {
        if (myCollider == null) return false;

        
        Bounds b = myCollider.bounds;
        
        Collider2D hit = Physics2D.OverlapBox(b.center, b.size, 0f, playerLayer);
        return hit != null;
    }

}
