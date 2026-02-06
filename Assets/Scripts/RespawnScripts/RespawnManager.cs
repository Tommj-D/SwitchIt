using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    private static List<RespawnableObject> allRespawnables = new List<RespawnableObject>();

    [Header("Player Respawn")]
    public Transform defaultRespawnPoint;
    private Transform currentRespawnPoint;

    private void Awake()
    {
        Instance = this;
        allRespawnables.Clear();
        currentRespawnPoint = defaultRespawnPoint;

        Debug.Log(
            $"[RespawnManager][Awake] Scene={gameObject.scene.name} | defaultRespawnPoint={(defaultRespawnPoint != null ? defaultRespawnPoint.name : "NULL")}"
        );
    }

    private void Start()
    {
        Debug.Log("[RespawnManager][Start]");

        // Spawn normale player solo se non in test mode
        if (GameManager.Instance != null && GameManager.Instance.isTestMode)
        {
            Debug.Log("[RespawnManager] TestMode attivo nella scena iniziale  NON spawno player");
            return;
        }

        StartCoroutine(SpawnPlayerDelayed());
    }

    private IEnumerator SpawnPlayerDelayed()
    {
        Debug.Log("[RespawnManager] Attendo 1 frame prima dello spawn...");
        yield return null; // piccolo delay per sicurezza

        PlayerRespawn player = Object.FindObjectOfType<PlayerRespawn>();
        Debug.Log($"[RespawnManager] Player trovato? {(player != null)}");

        Transform spawn = GetRespawnPoint();
        Debug.Log($"[RespawnManager] RespawnPoint = {(spawn != null ? spawn.name : "NULL")}");

        if (player == null || spawn == null)
        {
            Debug.LogWarning("[RespawnManager] Spawn ABORTITO");
            yield break;
        }

        Debug.Log("[RespawnManager] Chiamo ForceSpawn()");
        player.ForceSpawn(spawn);

        // Avvia animazione di spawn
        player.TriggerSpawnAnimation();
    }

    // =========================
    // REGISTRAZIONE OGGETTI
    // =========================
    public static void Register(RespawnableObject r)
    {
        if (!allRespawnables.Contains(r)) allRespawnables.Add(r);
    }

    public static void Unregister(RespawnableObject r)
    {
        if (allRespawnables.Contains(r)) allRespawnables.Remove(r);
    }

    // Resetta tutti gli oggetti registrati
    public void ResetAll()
    {
        for (int i = 0; i < allRespawnables.Count; i++)
        {
            var r = allRespawnables[i];
            if (r != null) r.ResetToStart();
        }
    }

    // =========================
    // PLAYER RESPAWN
    // =========================
    public void SetCheckpoint(Transform newPoint)
    {
        currentRespawnPoint = newPoint;
    }

    public Transform GetRespawnPoint()
    {
        return currentRespawnPoint != null ? currentRespawnPoint : defaultRespawnPoint;
    }
}
