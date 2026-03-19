using UnityEngine;

public class Slime_Blue : Enemy
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float jumpInterval = 2f;
    [SerializeField] private float preJumpDelay = 0.3f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private float jumpTimer = 0f;
    private bool isJumping = false;
    private bool isGrounded = false;
    private bool wasGrounded = false;
    private bool hasJumped = false;

    private Rigidbody2D rb;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Update()
    {
        base.Update();

        if (isDead || !isActive) return;

        CheckGround();
        HandleLanding(); 
        HandleJump();
        HandleRotation();

        wasGrounded = isGrounded;
    }

    protected override void Move()
    {
        // NON si muove mentre salta o prepara il salto
        if (isJumping) return;

        transform.Translate(Vector2.right * direction * patrolSpeed * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (isJumping) return;

        jumpTimer += Time.deltaTime;

        if (jumpTimer >= jumpInterval && isGrounded)
        {
            jumpTimer = 0f;
            StartCoroutine(JumpRoutine());
        }
    }

    private System.Collections.IEnumerator JumpRoutine()
    {
        isJumping = true;
        hasJumped = true;

        // Animazione preparazione (slime si abbassa)
        if (animator != null)
            animator.SetTrigger("Jump");

        // Aspetta un attimo (tempo animazione compressione)
        yield return new WaitForSeconds(preJumpDelay);

        // Salto
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction * patrolSpeed, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        //  Aspetta di tornare a terra
        yield return new WaitUntil(() => isGrounded);

        // Fine salto
        isJumping = false;
    }

    private void CheckGround()
    {
        if (groundCheck == null) return;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundRadius,
            groundLayer
        );
    }

    private void HandleRotation()
    {
        if (rb == null) return;

        // Inclina lo slime mentre è in aria
        if (!isGrounded)
        {
            float angle = Mathf.Clamp(rb.linearVelocity.y * -5f, -30f, 30f);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            // Reset rotazione a terra
            transform.rotation = Quaternion.identity;
        }
    }

    protected override void Sound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.slimeDeathSound);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }

    private void HandleLanding()
    {
        if (!wasGrounded && isGrounded && hasJumped)
        {
            if (animator != null)
            {
                animator.ResetTrigger("Land");
                animator.SetTrigger("Land");
            }

            hasJumped = false; // reset
        }
    }
}