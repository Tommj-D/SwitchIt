using UnityEngine;

public class Slime_Green : Enemy
{
    [Header("Direction (1 = right, -1 = left)")]
    [SerializeField] private int initialDirection = 1; 
    [SerializeField] private bool upsideDown;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.1f;
    [SerializeField] private LayerMask groundMask;

    protected bool isGrounded = false;

    protected override void Start()
    {
        base.Start();
        if (upsideDown)
        {
            transform.localScale = new Vector3(
                transform.localScale.x,
                -Mathf.Abs(transform.localScale.y),
                transform.localScale.z
            );
        }
        ApplyInitialDirection();
    }

    private void ApplyInitialDirection()
    {
        direction = initialDirection;

        if (sr != null)
            sr.flipX = direction < 0;
    }

    protected override void Update()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundRadius,
            groundMask
        );

        base.Update();
    }
    
    protected override void Move()
    {
        if (rb == null) return;

        rb.linearVelocity = new Vector2(direction * patrolSpeed, rb.linearVelocity.y);
    }

    protected override void Sound()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.slimeDeathSound);
    }

    public override void ResetEnemy()
    {
        base.ResetEnemy();
        ApplyInitialDirection();
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.CompareTag("Death"))
        {
            OnStomped();
            return;
        }

        //  OBSTACLE SOLO A TERRA
        if (collision.CompareTag("Obstacle") && !isGrounded)
            return;


        base.OnTriggerEnter2D(collision);
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

    public void SetGrounded(bool value)
    {
        isGrounded = value;
    }

    protected override void OnObstacleHit()
    {
        if (!isGrounded) return; 

        base.OnObstacleHit();
    }
}

