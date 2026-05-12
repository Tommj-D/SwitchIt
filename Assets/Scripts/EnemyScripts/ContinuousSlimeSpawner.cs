using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousSlimeSpawner : MonoBehaviour
{
    [Header("Impostazioni Slime")]
    public GameObject slimePrefab; 
    public float spawnInterval = 3f; 
    public int maxSlimesAlive = 3; 
    public float spawnRadius = 2f; 
    public Transform spawnCenter;  

    private List<GameObject> spawnedSlimes = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true) 
        {
            yield return new WaitForSeconds(spawnInterval);

            // 1. SPAWNA SOLO NEL MONDO FANTASTICO
            if (WorldSwitch.Instance != null && !WorldSwitch.Instance.isFantasyWorldActive)
                continue;

            spawnedSlimes.RemoveAll(slime => slime == null);

            if (spawnedSlimes.Count < maxSlimesAlive)
            {
                SpawnSingleSlime();
            }
        }
    }

    private void SpawnSingleSlime()
    {
        if (slimePrefab == null) return;

        Vector3 baseSpawnPos = spawnCenter != null ? spawnCenter.position : transform.position;
        float randomX = Random.Range(-spawnRadius, spawnRadius);
        
        // Nasce sopra, così ha il tempo di cadere dritto
        Vector3 targetPos = baseSpawnPos + new Vector3(randomX, 1.5f, 0f);

        GameObject slime = Instantiate(slimePrefab, targetPos, Quaternion.identity);
        
        // Allineiamo la profondità Z per sicurezza
        Vector3 pos = slime.transform.position;
        pos.z = transform.position.z;
        slime.transform.position = pos;

        // Miglioriamo la fisica per la caduta, ma SENZA aggiungerlo manualmente alla piattaforma.
        // Sarà la piattaforma stessa ad aggiungerlo quando lui ci sbatterà sopra i piedi.
        Rigidbody2D rbSlime = slime.GetComponent<Rigidbody2D>();
        if (rbSlime != null)
        {
            rbSlime.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rbSlime.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }
        
        spawnedSlimes.Add(slime);
    }

    public void ResetSpawner()
    {
        foreach (GameObject slime in spawnedSlimes)
            if (slime != null) Destroy(slime);
        spawnedSlimes.Clear();
    }
}