using System.Collections;
using UnityEngine;

public class Slime_Blue_Big : Slime_Blue
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

        base.OnStomped();
        SpawnSmalls();
    }

    private void SpawnSmalls()
    {
        if (smallEnemyPrefab == null) return;

        for (int i = 0; i < spawnCount; i++)
        {
            float t = spawnCount > 1 ? (float)i / (spawnCount - 1) : 0.5f;

            float offset = Mathf.Lerp(-spawnOffset, spawnOffset, t);
            Vector3 spawnPos = transform.position + new Vector3(offset, 0f, 0f);


            GameObject newEnemy = Instantiate(smallEnemyPrefab, spawnPos, Quaternion.identity);

            Slime_Blue slime = newEnemy.GetComponent<Slime_Blue>();
            Rigidbody2D newRb = newEnemy.GetComponent<Rigidbody2D>();

            float dir = (i == 0) ? -1f : 1f;

            if (slime != null)
            {
                slime.InitDirection((int)dir);   

                StartCoroutine(EnableMovementAfterDelay(slime, 0.2f));
            }

            if (newRb != null)
            {
                float randomX = Random.Range(0.9f, 1.3f);
                float randomY = Random.Range(1.0f, 1.4f);

                newRb.linearVelocity = new Vector2(
                    dir * launchForceX * randomX,
                    launchForceY * randomY
                );
            }
        }
    }

    private IEnumerator EnableMovementAfterDelay(Enemy enemy, float delay)
    {
        enemy.SetMovement(false);

        yield return new WaitForSeconds(delay);

        enemy.SetMovement(true);
    }
}