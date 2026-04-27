using System.Collections;
using System.Collections.Generic;
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

    // VARIABILI PER IL RESET
    private Vector3 startPosition;
    private List<GameObject> spawnedSlimes = new List<GameObject>();

    protected override void Start()
    {
        base.Start();
        startPosition = transform.position; 
    }

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
            float t = spawnCount > 1 ? (float)i / (spawnCount - 1) : 0.5f;

            float offset = Mathf.Lerp(-spawnOffset, spawnOffset, t);
            Vector3 spawnPos = transform.position + new Vector3(offset, 0f, 0f);

            GameObject newEnemy = Instantiate(smallEnemyPrefab, spawnPos, Quaternion.identity);
            
            // SALVIAMO IL CLONE NELLA LISTA
            spawnedSlimes.Add(newEnemy);

            Slime_Green slime = newEnemy.GetComponent<Slime_Green>();
            Rigidbody2D newRb = newEnemy.GetComponent<Rigidbody2D>();

            float dir = (i == 0) ? -1f : 1f;

            if (slime != null)
            {
                slime.InitDirection((int)dir);
                slime.SetGrounded(false);
                StartCoroutine(EnableMovementAfterDelay(slime, 0.2f));
            }

            if (newRb != null)
            {
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
        enemy.SetMovement(false);  
        yield return new WaitForSeconds(delay);
        enemy.SetMovement(true); 
    }

    public override void ResetEnemy()
    {
        base.ResetEnemy(); // Chiama il reset del padre

        foreach (GameObject smallSlime in spawnedSlimes)
        {
            if (smallSlime != null)
            {
                Destroy(smallSlime);
            }
        }
        spawnedSlimes.Clear();

        transform.position = startPosition; 
        isDead = false;
        gameObject.SetActive(true); 
    }
}