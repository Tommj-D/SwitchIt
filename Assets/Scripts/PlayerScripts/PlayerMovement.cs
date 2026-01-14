using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb; // riferimento al componente Rigidbody2D
    private SpriteRenderer sr; // riferimento al componente SpriteRenderer
    private Animator animator; // riferimento al componente Animator
    private SpriteMaskController spriteMaskController; // riferimento al componente SpriteMask

    [Header("Particle Systems")]
    public ParticleSystem grassFX; // riferimento all'effetto particellare di erba che solleva il player
    public ParticleSystem jumpFX; // riferimento all'effetto particellare di salto

    private float nextBlinkTime;

    [Header("Movement")]
    public float moveSpeed = 10f; //velocita di movimento orizzontale
    private float horizontalMovement;

    [Header("Jumping")]
    public float jumpPower = 5f;
    public int maxJumps = 2; // numero massimo di salti che il player può fare
    private int jumpsRemaining;
    private bool isGrounded;


    [Header("GroundCheck")]
    public Transform groundCheckPos; // punto di controllo a terra
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f); // dimensione del box di controllo
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;
    private int gravityDirection = 1; // 1 per normale, -1 per invertita

    [Header("Gravity Flip")]
    public float flipDuration = 0.25f;
    private bool isFlipping = false;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        spriteMaskController = GetComponentInChildren<SpriteMaskController>();
    }

    void Update()
    {
        rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
        GroundCeck();
        Gravity();

        ///ANIMAZIONI///
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetFloat("VerticalSpeed", Mathf.Abs(rb.linearVelocity.y));                          

        if (rb.linearVelocity.x == 0 && jumpsRemaining == maxJumps && Time.time >= nextBlinkTime)
        {
            animator.SetTrigger("Blink");
            nextBlinkTime = Time.time + Random.Range(3f, 6f);
        }

    }

    private void Gravity()
    {
        if (rb.linearVelocity.y * gravityDirection < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier * gravityDirection; // Aumenta la gravita' durante la caduta
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -maxFallSpeed, maxFallSpeed)); // Limita la velocita' di caduta
        }
        else
        {
            rb.gravityScale = baseGravity * gravityDirection; // Gravita' normale
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;

        if (Mathf.Abs(horizontalMovement) > 0.01f)
        {
            Flip(horizontalMovement);

            if (rb.linearVelocity.y == 0)
            {
                grassFX.Play();

                if (rb.linearVelocity.y != 0 && rb.linearVelocity.x == 0)
                {
                    grassFX.Stop();
                }
            }
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (jumpsRemaining>0)
        {
            {
                if (context.performed)
                {
                    if (jumpsRemaining < maxJumps)
                    {
                        animator.SetTrigger("DoubleJump");
                        jumpFX.Play();
                    }
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower*gravityDirection);
                    jumpsRemaining--;
                }
                else if (context.canceled)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
                }
            }
        }
    }

    private void GroundCeck()
    {
        bool groundedNow = Physics2D.OverlapBox(
        groundCheckPos.position,
        groundCheckSize,
        0,
        groundLayer
    );

        if (groundedNow && !isGrounded)
        {
            ResetJumps();
            animator.SetBool("isJumping", false);
        }

        isGrounded = groundedNow;
    }

    public void ResetJumps()
    {
        jumpsRemaining = maxJumps;
    }

    private void Flip(float direction)
    {
        if (direction > 0)
        {
            sr.flipX = false;
            grassFX.transform.localScale = new Vector3(1, 1, 1);
            jumpFX.transform.GetChild(0).localPosition = new Vector3(-0.2f, 0, 0);

            if (spriteMaskController != null)
                spriteMaskController.FaceRight();
        }
        else if (direction < 0)
        {
            sr.flipX = true;
            grassFX.transform.localScale = new Vector3(-1, 1, 1);
            jumpFX.transform.GetChild(0).localPosition = new Vector3(0.2f, 0, 0);

            if (spriteMaskController != null)
                spriteMaskController.FaceLeft();
        }
    }

    //----------- GRAVITY INVERSION -------//   
    public void InvertGravity(InputAction.CallbackContext context)
    {
        if(WorldSwitch.isFantasyWorldActive && !isFlipping) 
        {
            gravityDirection *= -1;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            StartCoroutine(SmoothFlip());
        }
    }

    IEnumerator SmoothFlip()
    {
        isFlipping = true;

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(180f, 0f, 0f);

        float elapsed = 0f;

        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flipDuration;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.rotation = targetRot;
        isFlipping = false;
    }
    //-------------------------------------//

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawCube(groundCheckPos.position, groundCheckSize);
    }
}
