using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousSlimeSpawner : MonoBehaviour
{
    [Header("Impostazioni Slime")]
    public GameObject slimePrefab;

    [Header("Tempo Spawn")]
    public float minSpawnTime = 1f;
    public float maxSpawnTime = 3f;

    [Header("Mondi Attivi")]
    public bool spawnInFantasyWorld = true;
    public bool spawnInRealWorld = false;

    [Header("Effetto Spawn")]
    public GameObject spawnParticlesPrefab;
    public float preSpawnDelay = 0.1f;

    [Header("Punti di Spawn")]
    public List<Transform> spawnPoints = new List<Transform>();

    // Lista slime spawnati
    private List<GameObject> spawnedSlimes = new List<GameObject>();

    private bool playerInsideTrigger = false;

    private Coroutine spawnCoroutine;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInsideTrigger = true;

            if (spawnCoroutine == null)
            {
                spawnCoroutine = StartCoroutine(SpawnRoutine());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInsideTrigger = false;

            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (playerInsideTrigger)
        {
            // Controllo mondo
            if (CanSpawnInCurrentWorld())
            {
                yield return StartCoroutine(SpawnSingleSlime());
            }

            // Tempo casuale
            float randomTime = Random.Range(minSpawnTime, maxSpawnTime);

            yield return new WaitForSeconds(randomTime);
        }
    }

    private bool CanSpawnInCurrentWorld()
    {
        if (WorldSwitch.Instance == null)
            return true;

        bool isFantasy = WorldSwitch.Instance.isFantasyWorldActive;

        // Fantasy
        if (isFantasy && spawnInFantasyWorld)
            return true;

        // Real
        if (!isFantasy && spawnInRealWorld)
            return true;

        return false;
    }

    private IEnumerator SpawnSingleSlime()
    {
        if (slimePrefab == null)
            yield break;

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("Nessun punto di spawn assegnato!");
            yield break;
        }

        // Punto casuale
        Transform randomPoint =
            spawnPoints[Random.Range(0, spawnPoints.Count)];

        Vector3 spawnPos = randomPoint.position;

        // Particelle pre-spawn
        if (spawnParticlesPrefab != null)
        {
            Instantiate(spawnParticlesPrefab, spawnPos, Quaternion.identity);
        }

        // Delay
        yield return new WaitForSeconds(preSpawnDelay);

        // Spawn slime
        GameObject slime =
            Instantiate(slimePrefab, spawnPos, Quaternion.identity);

        // Allinea Z
        Vector3 pos = slime.transform.position;
        pos.z = transform.position.z;
        slime.transform.position = pos;

        // Migliora fisica
        Rigidbody2D rbSlime = slime.GetComponent<Rigidbody2D>();

        if (rbSlime != null)
        {
            rbSlime.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;

            rbSlime.sleepMode =
                RigidbodySleepMode2D.NeverSleep;
        }

        // Rende lo slime figlio dello spawner
        slime.transform.SetParent(transform);
        spawnedSlimes.Add(slime);
    }

    // CHIAMA QUESTO ALLA MORTE DEL PLAYER
    public void ResetSpawner()
    {
        StopAllCoroutines();

        playerInsideTrigger = false;
        spawnCoroutine = null;

        foreach (GameObject slime in spawnedSlimes)
        {
            if (slime != null)
            {
                Destroy(slime);
            }
        }

        spawnedSlimes.Clear();
    }
}