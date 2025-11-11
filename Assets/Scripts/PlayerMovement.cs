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

    [SerializeField] private float checkRadius = 0.3f; // raggio del cerchio per verificare collisione
    [SerializeField] private LayerMask groundLayer; // layer dei terreni
    private bool isGrounded; // indica se il giocatore è a terra

    private bool isGravityInverted = false; // indica se la gravità è invertita



    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        controls = new PlayerControls();
    }


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
    }

    // Called when the object becomes disabled or inactive
    private void OnDisable()
    {
        if (controls != null)
            controls.Disable(); // Disable input actions
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
