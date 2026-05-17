using UnityEngine;
using System.Collections;

public class MiniSpawnBoss : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public Transform player;

    [Header("Hover")]
    public float hoverAmplitude = 0.5f;
    public float hoverFrequency = 2f;

    [Header("Dash")]
    public float dashSpeed = 10f;
    public float dashCooldown = 3f;
    public float chargeTime = 0.7f;
    public float dashDuration = 0.25f;

    [Header("Death")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip chaseSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem deathParticle;

    private bool isDead = false;
    private bool isDashing = false;
    private bool isCharging = false;

    private Collider2D col;
    private SpriteRenderer sprite;

    private float dashTimer;

    private Vector3 baseScale;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        dashTimer = dashCooldown;
        InvokeRepeating(nameof(PlaySound), 0f, 10f);
    }

    private void FixedUpdate()
    {
        if (isDead || player == null) return;

        dashTimer -= Time.deltaTime;

        if (!isDashing && !isCharging)
        {
            HoverMovement();

            if (dashTimer <= 0f)
            {
                StartCoroutine(ChargeAndDash());
                dashTimer = dashCooldown;
            }
        }
    }

    private void HoverMovement()
    {
        Vector3 dir = (player.position - transform.position).normalized;

        // 👉 movimento
        Vector3 hover = Vector3.up * Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        transform.position += (dir * moveSpeed * Time.deltaTime) + hover * Time.deltaTime;

        // 👉 FLIP SINISTRA/DESTRA
        if (dir.x > 0.01f)
            transform.localScale = new Vector3(Mathf.Abs(baseScale.x), baseScale.y, baseScale.z);
        else if (dir.x < -0.01f)
            transform.localScale = new Vector3(-Mathf.Abs(baseScale.x), baseScale.y, baseScale.z);
    }

    private IEnumerator ChargeAndDash()
    {
        isCharging = true;

        yield return new WaitForSeconds(chargeTime);

        isCharging = false;
        isDashing = true;

        Vector3 direction = (player.position - transform.position).normalized;
        float timer = 0f;

        while (timer < dashDuration)
        {
            transform.position += direction * dashSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    private void PlaySound()
    {
        if (!isDead && audioSource != null && chaseSound != null)
            audioSource.PlayOneShot(chaseSound);
    }

    // ❌ RIMOSSO: collision kill col player

    public void Die()   // 👉 chiamalo dallo script della testa
    {
        if (isDead) return;

        isDead = true;

        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound);

        if (deathParticle != null)
            Instantiate(deathParticle, transform.position, transform.rotation).Play();

        if (col != null) col.enabled = false;
        if (sprite != null) sprite.enabled = false;

        Destroy(gameObject, 0.5f);
    }
}