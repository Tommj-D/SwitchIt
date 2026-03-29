using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine; // Assicurati che CinemachineCamera sia disponibile

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb; // riferimento al componente Rigidbody2D
    private SpriteRenderer sr; // riferimento al componente SpriteRenderer
    private Animator animator; // riferimento al componente Animator
    private SpriteMaskController spriteMaskController; // riferimento al componente SpriteMask

    [Header("Particle Systems")]
    public ParticleSystem grassFX; // riferimento all'effetto particellare di erba che solleva il player
    public ParticleSystem jumpFX; // riferimento all'effetto particellare di salto
    
    [Header("Particle Colors")]
    public Color fantasyGrassColor = Color.magenta;
    public Color fantasyJumpColor = Color.cyan;

    // Colori della dimensione reale (quelli originali)
    private Color realGrassColor;
    private Color realJumpColor;

    private float nextBlinkTime;

    [Header("Movement")]
    public float moveSpeed = 10f; //velocita di movimento orizzontale
    private float horizontalMovement;
    public static bool isFacingRight = true;

    [Header("Jumping")]
    public float jumpPower = 5f;
    public int maxJumps = 2; // numero massimo di salti che il player può fare
    private int jumpsRemaining;
    private bool isGrounded;
    private bool isJumping = false;
    [Header("Fantasy Jump")]
    [Range(0.5f, 1f)]
    public float fantasyJumpMultiplier = 0.85f;


    [Header("GroundCheck")]
    public Transform groundCheckPos; // punto di controllo a terra
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f); // dimensione del box di controllo
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;
    private int gravityDirection = 1; // 1 per normale, -1 per invertita
    private bool canFlipGravity = true;
    private float gravityFlipCooldown = 0.2f;
    private float lastFlipTime;
    [Header("Fantasy Gravity")]
    [Range(0.1f, 1f)]
    public float fantasyGravityMultiplier = 0.7f; // gravità ridotta nel mondo fantasy

    [Header("Gravity Flip")]
    public float flipDuration = 0.25f;
    private bool isFlipping = false;

    [Header("Camera")]
    public CinemachineCamera vcam;
    public float cameraOffsetX = 4f;

    private float offsetSmoothTime = 0.3f; //Quanto velocemante la cam segue il player 
    private float targetOffsetX;
    private float currentOffsetX;
    private Vector3 cameraBaseOffset;
    private float offsetVelocity;
    private CinemachinePositionComposer positionComposer;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        spriteMaskController = GetComponentInChildren<SpriteMaskController>();

        if (vcam != null)
        {
            positionComposer = vcam.GetComponent<CinemachinePositionComposer>();

            if (positionComposer != null)
            {
                cameraBaseOffset = positionComposer.TargetOffset;

                currentOffsetX = 0f;
                targetOffsetX = 0f;
            }
            currentOffsetX = cameraOffsetX;
            targetOffsetX = cameraOffsetX;
        }
        realGrassColor = grassFX.main.startColor.color;
        realJumpColor = jumpFX.main.startColor.color;
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
        UpdateCameraOffset();
    }

    private void Gravity()
    {
        float gravityMultiplier = 1f;

        if (WorldSwitch.Instance != null && WorldSwitch.Instance.isFantasyWorldActive)
            gravityMultiplier = fantasyGravityMultiplier;

        float finalGravity = baseGravity * gravityMultiplier;

        if (rb.linearVelocity.y * gravityDirection < 0)
        {
            rb.gravityScale = finalGravity * fallSpeedMultiplier * gravityDirection;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -maxFallSpeed, maxFallSpeed));
        }
        else
        {
            rb.gravityScale = finalGravity * gravityDirection;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;

        if (Mathf.Abs(horizontalMovement) > 0.01f)
        {   
            
            Flip(horizontalMovement);

            if (isGrounded)
            {
                var grassMain = grassFX.main;
                if(WorldSwitch.Instance!=null)
                    grassMain.startColor = WorldSwitch.Instance.isFantasyWorldActive ? fantasyGrassColor : realGrassColor;

                grassFX.Play();
                if (AudioManager.Instance.walkSound != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.walkSound);
                }
            }
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (jumpsRemaining > 0)
        {
            isJumping = true;
            {
                if (context.performed)
                {
                    if (jumpsRemaining < maxJumps)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.jumpSound);
                        animator.SetTrigger("DoubleJump");

                        var jumpMain = jumpFX.main;
                        if (WorldSwitch.Instance != null)
                            jumpMain.startColor = WorldSwitch.Instance.isFantasyWorldActive ? fantasyJumpColor : realJumpColor;

                        jumpFX.Play();
                    }
                    float jumpMultiplier = 1f;

                    if (WorldSwitch.Instance != null && WorldSwitch.Instance.isFantasyWorldActive)
                        jumpMultiplier = fantasyJumpMultiplier;

                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower * jumpMultiplier * gravityDirection);
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
            if (isJumping)
            {
                if (AudioManager.Instance != null && AudioManager.Instance.jumpLanding != null)
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.jumpLanding);
                isJumping = false;
            }
            ResetJumps();
            animator.SetBool("isJumping", false);
        }

        if (groundedNow && Time.time > lastFlipTime + gravityFlipCooldown)
        {
            canFlipGravity = true;
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
            isFacingRight = true;

            targetOffsetX = cameraOffsetX; //Flip camera
        }
        else if (direction < 0)
        {
            sr.flipX = true;
            grassFX.transform.localScale = new Vector3(-1, 1, 1);
            jumpFX.transform.GetChild(0).localPosition = new Vector3(0.2f, 0, 0);
            isFacingRight = false;

            targetOffsetX = -cameraOffsetX; //Flip Camera
        }
    }

    public void InvertGravity(InputAction.CallbackContext context)
    {
        if (!context.performed) return; 

        if (WorldSwitch.Instance != null && WorldSwitch.Instance.isFantasyWorldActive && !isFlipping && WorldSwitch.Instance.canSwitchGravity && canFlipGravity)
        {
            gravityDirection *= -1;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

            canFlipGravity = false;
            lastFlipTime = Time.time; 

            StartCoroutine(SmoothFlip());
        }
    }

    public bool IsGravityInverted()
    {
        return gravityDirection < 0;
    }

    public void ResetGravity()
    {
        if (gravityDirection < 0)
        {
            gravityDirection = 1;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

            StopAllCoroutines();
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

    //----------- WORLD SWITCH-----------//
    public void DimensionSwitch(InputAction.CallbackContext context)
    {
        if (!context.performed || GetComponent<PlayerRespawn>().IsDying() || GameManager.Instance.isChangingLevel ||WorldSwitch.Instance==null) return;

        WorldSwitch.Instance.SwitchWorld();
    }

    //----------- CAMERA OFFSET/SETTINGS -----------//
    private void UpdateCameraOffset()
    {
        if (positionComposer == null) return;

        currentOffsetX = Mathf.SmoothDamp(
            currentOffsetX,
            targetOffsetX,
            ref offsetVelocity,
            offsetSmoothTime
        );

        positionComposer.TargetOffset = new Vector3(
            cameraBaseOffset.x + currentOffsetX,
            cameraBaseOffset.y,
            cameraBaseOffset.z
        );
    }

    public void ResetCameraOffset()
    {
        targetOffsetX = 0f;
    }

    //----------- GIZMOS -----------//
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawCube(groundCheckPos.position, groundCheckSize);
    }
}
