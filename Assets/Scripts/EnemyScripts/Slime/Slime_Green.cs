using UnityEngine;

public class Slime_Green : Enemy
{
    protected override void Move()
    {
        transform.Translate(Vector2.right * direction * patrolSpeed * Time.deltaTime);
    }
}

