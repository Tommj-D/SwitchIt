using UnityEngine;

public class Slime_Green_Big : Slime_Green
{
    [Header("Split Settings")]
    [SerializeField] private GameObject smallEnemyPrefab;
    [SerializeField] private int spawnCount = 2;
    [SerializeField] private float spawnOffset = 0.5f;

    protected override void Move()
    {
        // Puoi riutilizzare lo stesso movimento dello slime
        rb.linearVelocity = new Vector2(direction * patrolSpeed, rb.linearVelocity.y);
    }

    protected override void Sound()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.slimeDeathSound);
    }

    public override void OnStomped()
    {
        if (isDead) return;

        
        SpawnSmalls();

        base.OnStomped();
    }

    private void SpawnSmalls()
    {
        if (smallEnemyPrefab == null) return;

        for (int i = 0; i < spawnCount; i++)
        {
            float offset = (i == 0 ? -spawnOffset : spawnOffset);

            Vector3 spawnPos = transform.position + new Vector3(offset, 0f, 0f);

            GameObject newEnemy = Instantiate(smallEnemyPrefab, spawnPos, Quaternion.identity);

            // 👇 opzionale: dai direzioni opposte
            Enemy enemyScript = newEnemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                int dir = (i == 0) ? -1 : 1;
                enemyScript.SetDirection(dir);
            }
        }
    }
}