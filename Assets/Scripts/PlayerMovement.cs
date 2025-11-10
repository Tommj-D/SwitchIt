using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Velocit� di movimento orizzontale
    public float jumpForce = 7f; // Forza del salto

    public Transform groundCheck; // Punto di controllo per il terreno
    public LayerMask groundLayer; // Layer del terreno              

    private Rigidbody2D rb; // Riferimento al Rigidbody2D del giocatore
    private float moveInput; // Input di movimento orizzontale
    private bool jumpPressed; // Stato del tasto di salto
    private bool isGrounded; // Stato di contatto con il terreno

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        Debug.Log(moveInput);
    }

    // Movimenti personaggio
    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }
}
