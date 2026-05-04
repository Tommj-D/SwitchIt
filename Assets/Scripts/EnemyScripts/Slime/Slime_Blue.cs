using System.Collections;
using UnityEngine;

public class Slime_Blue : Enemy
{

    [Header("Direction (1 = right, -1 = left)")]
    [SerializeField] private int initialDirection = 1;

    [Header("Inverted Gravity")]
    [SerializeField] private bool isInverted = false;

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
    private bool justLanded;

    protected override void Start()
    {
        base.Start();

        ApplyInitialDirection();

        nextJumpTime = Random.Range(jumpIntervalMin, jumpIntervalMax);
    }

    private void ApplyInitialDirection()
    {
        direction = initialDirection;

        if (sr != null)
            sr.flipX = direction < 0;
    }

    protected override void Update()
    {
        base.Update();

        if (isDead || !isActive) return;

        CheckGround(); 
        HandleJump();
        HandleLanding();
        
        HandleRotation();

        wasGrounded = isGrounded;
    }

    protected override void Move()
    {
        if (isJumping) return;

        // Movimento orizzontale puro
        transform.Translate(Vector2.right * direction * patrolSpeed * Time.deltaTime, Space.World);
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {   
            //Se lo slime atterra sulle spine dopo un salto muore
            if (collision.gameObject.CompareTag("Spike"))
            {
                if (!isGrounded && rb != null && rb.linearVelocity.y < 0f)
                {
                    OnStomped();
                    return;
                }
            }
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground_Back"))
            return;

        
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Enemy"))
        {
            HandleWallCollision(collision);
        }
        else
        {
            base.OnCollisionEnter2D(collision);
        }
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

        float gravityDirection = isInverted ? -1f : 1f;
        rb.AddForce(Vector2.up * jumpForce * gravityDirection, ForceMode2D.Impulse);
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

        //SALVA X e Y attuali
        Vector3 currentRotation = transform.rotation.eulerAngles;

        transform.rotation = Quaternion.Euler(
            currentRotation.x,   // mantiene 180
            currentRotation.y,
            zRotation
        );
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
            justLanded = true;
            StartCoroutine(ResetJustLanded());
        }
    }

    private IEnumerator ResetJustLanded()
    {
        yield return new WaitForFixedUpdate();
        justLanded = false;
    }

    protected override void OnObstacleHit()
    {
        // NON girarti se:
        if (!isGrounded) return;     // sei in aria
        if (justLanded) return;      // hai appena toccato terra

        Flip();
    }

    public override void ResetEnemy()
    {
        base.ResetEnemy();

        StopAllCoroutines();

        ApplyInitialDirection();

        // Reset stato salto
        isJumping = false;
        hasJumped = false;
        hasPerformedJump = false;

        jumpTimer = 0f;
        nextJumpTime = Random.Range(jumpIntervalMin, jumpIntervalMax);

        // Reset velocità
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Reset rotazione
        transform.rotation = Quaternion.identity;
    }

    public void InitDirection(int dir)
    {
        initialDirection = dir;
        direction = dir;

        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (sr != null)
            sr.flipX = direction < 0;
    }

    // Gestione collisione con muri e ostacoli hittati mnetre lo slime è in aria o a terra
    private void HandleWallCollision(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // controlliamo se è davvero un muro (non pavimento)
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                // ignora subito dopo atterraggio
                if (justLanded)
                    return;

                // IN ARIA → rimbalzo
                if (!isGrounded && rb != null)
                {
                    rb.linearVelocity = new Vector2(-direction * jumpHorizontalSpeed, rb.linearVelocity.y);
                    return;
                }

                // A TERRA → gira
                if (isGrounded)
                {
                    Flip();
                    return;
                }
            }
        }
    }
}