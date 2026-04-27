using System.Collections;
using System.Collections.Generic; // Serve per usare le List
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

    // VARIABILI PER IL RESET
    private Vector3 startPosition;
    private List<GameObject> spawnedSlimes = new List<GameObject>();

    protected override void Start() // Assicurati che nel base script Start sia "protected virtual void Start()"
    {
        base.Start();
        // Salviamo la posizione in cui si trova all'inizio del livello
        startPosition = transform.position; 
    }

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
            
            // AGGIUNGIAMO IL CLONE ALLA LISTA PER RICORDARCELO
            spawnedSlimes.Add(newEnemy);

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

    public override void ResetEnemy()
    {
        // 1. Chiama il reset originale del padre (es. ridà la vita, riattiva collider)
        base.ResetEnemy(); 

        // 2. Distrugge tutti i piccoli slime creati
        foreach (GameObject smallSlime in spawnedSlimes)
        {
            if (smallSlime != null)
            {
                Destroy(smallSlime);
            }
        }
        spawnedSlimes.Clear(); // Pulisce la lista

        // 3. Riporta in vita il grande slime
        transform.position = startPosition; // Lo rimette al suo posto
        isDead = false;
        gameObject.SetActive(true); 
    }
}