using System.Collections;
using UnityEngine;

public class Slime_Green_Big : Slime_Green
{
    [Header("Split Settings")]
    [SerializeField] private GameObject smallEnemyPrefab;
    [SerializeField] private int spawnCount = 2;
    [SerializeField] private float spawnOffset = 0.5f;

    [Header("Launch Settings")]
    [SerializeField] private float launchForceX = 6f;
    [SerializeField] private float launchForceY = 8f;

    public override void OnStomped()
    {
        if (isDead) return;

        base.OnStomped(); // animazione + stato
        SpawnSmalls();
    }

    private void SpawnSmalls()
    {
        if (smallEnemyPrefab == null) return;

        for (int i = 0; i < spawnCount; i++)
        {
            float offset = (i == 0 ? -spawnOffset : spawnOffset);
            Vector3 spawnPos = transform.position + new Vector3(offset, 0f, 0f);

            GameObject newEnemy = Instantiate(smallEnemyPrefab, spawnPos, Quaternion.identity);

            Slime_Green slime = newEnemy.GetComponent<Slime_Green>();
            Rigidbody2D newRb = newEnemy.GetComponent<Rigidbody2D>();

            float dir = (i == 0) ? -1f : 1f;

            if (slime != null)
            {
                slime.InitDirection((int)dir);

                // blocca movimento
                StartCoroutine(EnableMovementAfterDelay(slime, 0.2f));
            }

            if (newRb != null)
            {
                //VELOCITÀ DIRETTA (meglio di AddForce)
                float randomX = Random.Range(0.9f, 1.3f);
                float randomY = Random.Range(1.0f, 1.4f);

                Vector2 velocity = new Vector2(
                    dir * launchForceX * randomX,
                    launchForceY * randomY
                );

                newRb.linearVelocity = velocity;
            }
        }
    }

    private IEnumerator EnableMovementAfterDelay(Enemy enemy, float delay)
    {
        enemy.enabled = false; // blocca Move()

        yield return new WaitForSeconds(delay);

        enemy.enabled = true; // riattiva
    }
}