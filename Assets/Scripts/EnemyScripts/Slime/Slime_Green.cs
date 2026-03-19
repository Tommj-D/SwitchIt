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
        transform.Translate(Vector2.right * direction * patrolSpeed * Time.deltaTime);
    }

    protected override void Sound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.slimeDeathSound);
        }
    }
}

