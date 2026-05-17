using UnityEngine;

public class FlyingMinionParticle : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float arriveDistance = 0.15f;

    [Header("Effetti e Prefab")]
    [SerializeField] private ParticleSystem burstParticle;
    [SerializeField] private GameObject minionPrefab;

    private Transform targetPoint;

    // Questa funzione viene chiamata dal Boss per dare le coordinate
    public void Setup(Transform target)
    {
        targetPoint = target;
    }

    private void Update()
    {
        // Se non c'è un target, si distrugge per sicurezza
        if (targetPoint == null)
        {
            Destroy(gameObject);
            return;
        }

        // Muove la particella verso il punto scelto
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            speed * Time.deltaTime
        );

        // Controlla se è arrivata
        float distance = Vector2.Distance(transform.position, targetPoint.position);

        if (distance <= arriveDistance)
        {
            // 1. Spawna l'esplosione (burst)
            if (burstParticle != null)
            {
                Instantiate(burstParticle, transform.position, Quaternion.identity);
            }

            // 2. Spawna il minion
            if (minionPrefab != null)
            {
                Instantiate(minionPrefab, transform.position, Quaternion.identity);
            }

            // 3. Distrugge la particella volante
            Destroy(gameObject);
        }
    }
}