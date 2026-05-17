using UnityEngine;

public class FlyingRewardParticle : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float speed = 8f;

    [SerializeField] private float arriveDistance = 0.15f;

    private Transform target;
    private RewardTarget rewardTarget;

    public void Setup(
        Transform newTarget,
        RewardTarget newRewardTarget
    )
    {
        target = newTarget;
        rewardTarget = newRewardTarget;
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

        if (distance <= arriveDistance)
        {
            if (rewardTarget != null)
            {
                rewardTarget.Reveal();
            }

            Destroy(gameObject);
        }
    }
}