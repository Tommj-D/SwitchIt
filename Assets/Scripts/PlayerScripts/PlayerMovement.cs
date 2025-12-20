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
    private float moveInput; // input di movimento orizzontale va da -1 a 1

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
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier; // Aumenta la gravita' durante la caduta
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed)); // Limita la velocita' di caduta
        }
        else
        {
            rb.gravityScale = baseGravity; // Gravita' normale
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
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
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

    public void ResetJumps()
    {
        jumpsRemaining = maxJumps;
    }


    /*[SerializeField] private Transform groundCheckLeft; // riferimento al GameObject
    [SerializeField] private Transform groundCheckRight; // riferimento al GameObject
    [SerializeField] private Transform groundCheckCenter; // riferimento al GameObject

    [SerializeField] private float checkRadius = 0.15f; // raggio del cerchio per verificare collisione
    [SerializeField] private LayerMask groundLayer; // layer dei terreni
    private bool isGrounded; // indica se il giocatore � a terra
    private bool canDoubleJump = false; // indica se il giocatore pu� fare un doppio salto

    private bool isGravityInverted = false; // indica se la gravit� � invertita
    [SerializeField] private float gravityCooldown = 0.5f; // mezzo secondo di attesa
    private float lastGravitySwitchTime = -10f; //inizializza cos� da permettere il primo switch subito

    private float baseGravityScale; // per salvare la gravit� originale


    //Per animazione di blink
    [SerializeField] private float minBlinkTime = 3f;
    [SerializeField] private float maxBlinkTime = 6f;
    private float nextBlinkTime;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        controls = new PlayerControls();
    }


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // Ottieni il componente Animator
        sr = GetComponent<SpriteRenderer>(); // Ottieni il componente SpriteRenderer
        baseGravityScale = rb.gravityScale;

        //Per animazione blink
        ScheduleNextBlink();
    }

    // Update is called once per frame
    void Update()
    {
 
        isGrounded = Physics2D.OverlapCircle(groundCheckLeft.position, checkRadius, groundLayer) ||
        Physics2D.OverlapCircle(groundCheckRight.position, checkRadius, groundLayer) ||
        Physics2D.OverlapCircle(groundCheckCenter.position, checkRadius, groundLayer);
        
        // Movimento orizzontale ///
        Vector2 move = controls.Player.Move.ReadValue<Vector2>();
        moveInput = move.x;

        if (moveInput > 0.01f)
        {
            sr.flipX = false;
        }
        else if (moveInput < -0.01f)
        {
            sr.flipX = true;
        }
        //Aggiorna animazioni movimento orizziontale
        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
        // Imposta velocit� orizzontale 
        anim.SetFloat("Speed", horizontalSpeed);

        /// Salto ///
        if (controls.Player.Jump.WasPressedThisFrame())
        {
            if (isGrounded)
            {
                if (!isGravityInverted)
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                else
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, -jumpForce);

                anim.SetBool("isJumping", true); // Aggiorna animazione salto

                canDoubleJump = true; // Abilita il doppio salto
            }
            else if (canDoubleJump)
            {
                if (!isGravityInverted)
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                else
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, -jumpForce);

                canDoubleJump = false; // Consuma il doppio salto
                anim.SetTrigger("DoubleJump"); // Attiva animazione doppio salto
            }
        }

        anim.SetBool("isJumping", !isGrounded);

        /// Cambio gravit� ///
        if (controls.Player.InvertGravity.WasPressedThisFrame()&&WorldSwitch.isFantasyWorldActive)
        {
            if (Time.time - lastGravitySwitchTime >= gravityCooldown)
            {
                isGravityInverted = !isGravityInverted;
                rb.gravityScale *= -1f;
                lastGravitySwitchTime = Time.time;
            }
        }


        /// Gestione animazione Blink ///
        // Controlla se il personaggio � fermo
        bool isIdle = horizontalSpeed < 0.01f && isGrounded; // fermo e a terra
        if (isIdle && Time.time >= nextBlinkTime)
        {
            anim.SetTrigger("Blink");
            ScheduleNextBlink();
        }
    }

    // FixedUpdate is called at fixed intervals and is used for physics updates
    private void FixedUpdate()
    {
        //Applica velocit� orizzontale mantenendo la velocit� verticale
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    // Imposta il prossimo Blink casuale
    private void ScheduleNextBlink()
    {
        nextBlinkTime = Time.time + Random.Range(minBlinkTime, maxBlinkTime);
    }

    // Called when the object becomes enabled and active
    private void OnEnable()
    {
        if (controls != null)
            controls.Enable(); // Enable input actions

        //Mi iscrivo all�evento del cambio mondo
        WorldSwitch.OnWorldChanged += HandleWorldChange;
    }

    // Called when the object becomes disabled or inactive
    private void OnDisable()
    {
        if (controls != null)
            controls.Disable(); // Disable input actions

        //Mi disiscrivo per sicurezza
        WorldSwitch.OnWorldChanged -= HandleWorldChange;
    }

    private void HandleWorldChange(bool isFantasyActive)
    {
        if (!isFantasyActive && isGravityInverted)
        {
            isGravityInverted = false;
            rb.gravityScale = baseGravityScale;
        }
    }
    */

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawCube(groundCheckPos.position, groundCheckSize);
    }
}
