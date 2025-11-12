using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    
    private PlayerControls controls; // riferimento alla classe PlayerControls generata


    private Rigidbody2D rb; // riferimento al componente Rigidbody2D


    private float moveInput; // input di movimento orizzontale va da -1 a 1

    [SerializeField] private float moveSpeed = 10f; //velocit� di movimento orizzontale
    [SerializeField] private float jumpForce = 5f;

    [SerializeField] private Transform groundCheckDown; // riferimento al GameObject
    [SerializeField] private Transform groundCheckUp; // riferimento al GameObject

    [SerializeField] private float checkRadius = 0.15f; // raggio del cerchio per verificare collisione
    [SerializeField] private LayerMask groundLayer; // layer dei terreni
    private bool isGrounded; // indica se il giocatore è a terra

    private bool isGravityInverted = false; // indica se la gravità è invertita
    [SerializeField] private float gravityCooldown = 0.5f; // mezzo secondo di attesa
    private float lastGravitySwitchTime = -10f; //inizializza così da permettere il primo switch subito

    private float baseGravityScale; // per salvare la gravità originale



    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        controls = new PlayerControls();
    }


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        baseGravityScale = rb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        Transform activeGroundCheck = null;
        if (isGravityInverted==false)
        {
            activeGroundCheck = groundCheckDown;
        }
        else
        {
            activeGroundCheck = groundCheckUp;
        }

        // Aggiorna stato a terra
        if (activeGroundCheck != null)
        {

            isGrounded = Physics2D.OverlapCircle(activeGroundCheck.position, checkRadius, groundLayer);
        }

        // Movimento orizzontale
        Vector2 move = controls.Player.Move.ReadValue<Vector2>();
        moveInput = move.x;

        // Salto
        if (isGrounded && controls.Player.Jump.WasPressedThisFrame())
        {
            if(isGravityInverted==false)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            else
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -jumpForce);
        }

        //Cambio gravità
        if(controls.Player.InvertGravity.WasPressedThisFrame()&&WorldSwitch.isFantasyWorldActive)
        {
            if (Time.time - lastGravitySwitchTime >= gravityCooldown)
            {
                isGravityInverted = !isGravityInverted;
                rb.gravityScale *= -1f;
                lastGravitySwitchTime = Time.time;
            }
        }
    }

    // FixedUpdate is called at fixed intervals and is used for physics updates
    private void FixedUpdate()
    {
        //Applica velocità orizzontale mantenendo la velocità verticale
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }


    // Called when the object becomes enabled and active
    private void OnEnable()
    {
        if (controls != null)
            controls.Enable(); // Enable input actions

        //Mi iscrivo all’evento del cambio mondo
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

    private void OnDrawGizmosSelected()
    {
        if (groundCheckDown != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckDown.position, checkRadius);
        }
        if (groundCheckUp != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckUp.position, checkRadius);
        }
    }
}
