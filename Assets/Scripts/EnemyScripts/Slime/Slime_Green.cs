using UnityEngine;

public class Slime_Green : Enemy
{
    protected override void Move()
    {
        transform.Translate(Vector2.right * direction * patrolSpeed * Time.deltaTime);
    }

    protected override void Sound()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(AudioManager.instance.slimeDeathSound);
        }
    }
}

