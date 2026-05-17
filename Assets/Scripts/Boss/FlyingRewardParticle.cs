using UnityEngine;

public class FlyingRewardParticle : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float speed = 6f;

    private Transform target;
    private RewardRevealObject revealObject;

    public void Setup(
        Transform newTarget,
        RewardRevealObject newRevealObject
    )
    {
        target = newTarget;
        revealObject = newRevealObject;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        float distance = Vector2.Distance(
            transform.position,
            target.position
        );

        if (distance < 0.15f)
        {
            if (revealObject != null)
            {
                revealObject.Reveal();
            }

            Destroy(gameObject);
        }
    }
}