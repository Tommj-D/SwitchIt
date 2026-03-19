using UnityEngine;

public class Slime_Green : Enemy
{
    [Header("Direction (1 = right, -1 = left)")]
    [SerializeField] private int initialDirection = 1;

    protected override void Start()
    {
        base.Start();
        direction = initialDirection;
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
}

