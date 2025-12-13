using UnityEngine;

public class Slime_Red : MonoBehaviour
{
    public Transform player;
    public float chaseSpeed = 2f;
    public float jumpForce = 2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool shouldJump;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer);

        //player direction
        float direction = Mathf.Sign(player.position.x - transform.position.x);

        bool isPlayerAbove = Physics2D.Raycast(transform.position, Vector2.up, 3f, LayerMask.GetMask("Player"));

        if(isGrounded)
        {
            //chase player
            rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

            //Salta se c'è uno spazio e non c'è terreno davnt, o se il player è sopra
            RaycastHit2D obstacleInFront = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 2f, groundLayer);
            RaycastHit2D gapAhead = Physics2D.Raycast(transform.position + new Vector3(direction, 0, 0), Vector2.down, 2f, groundLayer);
           
            if (!gapAhead.collider || obstacleInFront.collider || isPlayerAbove)
            {
                shouldJump = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isGrounded && shouldJump)
        {
            shouldJump = false;
            
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }
    }
}
