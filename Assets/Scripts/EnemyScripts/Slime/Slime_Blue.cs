using System.Collections;
using UnityEngine;

public class Slime_Blue : Enemy
{
    [Header("Direction (1 = right, -1 = left)")]
    [SerializeField] private int initialDirection = 1;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float jumpIntervalMin = 2f;
    [SerializeField] private float jumpIntervalMax = 4f;
    [SerializeField] private float jumpHorizontalSpeed = 5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private float jumpTimer = 0f;
    private bool isJumping = false;
    private bool isGrounded = false;
    private bool wasGrounded = false;
    private bool hasJumped = false;
    private float nextJumpTime = 0f;
    private bool hasPerformedJump = false;

    protected override void Start()
    {
        base.Start();
        direction = initialDirection;
        if (sr != null)
            sr.flipX = direction < 0;
            
        nextJumpTime = Random.Range(jumpIntervalMin, jumpIntervalMax);
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
        if (isJumping) return;

        // Movimento orizzontale puro
        transform.Translate(Vector2.right * direction * patrolSpeed * Time.deltaTime, Space.World);
    }

    private void HandleJump()
    {
        if (isJumping) return;

        jumpTimer += Time.deltaTime;

        if (jumpTimer >= nextJumpTime && isGrounded)
        {
            jumpTimer = 0f;

            isJumping = true;
            hasPerformedJump = false; 

            StartCoroutine(JumpRoutine());

            nextJumpTime = Random.Range(jumpIntervalMin, jumpIntervalMax);
        }
    }

    private IEnumerator JumpRoutine()
    {
        hasJumped = true;

        // Animazione preparazione
        animator?.SetTrigger("PrepareJump");

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        //aspetta che l'animation event faccia il salto
        yield return new WaitUntil(() => hasPerformedJump);

        // Aspetta di tornare a terra
        yield return new WaitUntil(() => isGrounded);

        isJumping = false;
    }

    public void PerformJump()
    {
        if (rb == null) return;

        hasPerformedJump = true;

        animator?.SetTrigger("Jump");

        rb.linearVelocity = new Vector2(direction * jumpHorizontalSpeed, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
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

        float zRotation = 0f;

        if (!isGrounded)
        {
            zRotation = Mathf.Clamp(rb.linearVelocity.y * -5f, -30f, 30f);
        }

        // Mantieni la rotazione Y (direzione), cambia solo Z
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, zRotation);
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