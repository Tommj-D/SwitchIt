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

    [Header("Camera Settings")]
    public CameraConfinerManager cameraConfinerManager;
    public Collider2D defaultConfiner; // Il confiner iniziale del livello
    private Collider2D currentConfiner; // Il confiner associato al checkpoint salvato

    private Checkpoint currentCheckpoint;

    private void Awake()
    {
        Instance = this;
        currentRespawnPoint = defaultRespawnPoint;
        currentConfiner = defaultConfiner; // All'inizio usa quello di default
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.isTestMode)
        {
            return;
        }

        StartCoroutine(SpawnPlayerDelayed());
    }

    private IEnumerator SpawnPlayerDelayed()
    {
        yield return null; 

        PlayerRespawn player = Object.FindFirstObjectByType<PlayerRespawn>();
        
        Transform spawn = GetRespawnPoint();
        
        if (player == null || spawn == null)
        {
            yield break;
        }

        player.ForceSpawn(spawn);
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

    public void ResetAll()
    {
        for (int i = 0; i < allRespawnables.Count; i++)
        {
            var r = allRespawnables[i];
            if (r != null)
            {
                r.ResetToStart();
            }
        }
    }

    // =========================
    // PLAYER RESPAWN
    // =========================
    
    // Modificato: Ora accetta anche il confiner del checkpoint!
    public void SetCheckpoint(Transform newPoint, Collider2D newConfiner = null)
    {
        currentRespawnPoint = newPoint;
        
        currentCheckpoint = newPoint.GetComponent<Checkpoint>();

        if (newConfiner != null)
        {
            currentConfiner = newConfiner;
        }
    }

    public Checkpoint GetCurrentCheckpoint()
    {
        return currentCheckpoint;
    }

    public Transform GetRespawnPoint()
    {
        // Quando qualcuno chiede dove rinascere (alla morte), resettiamo la camera!
        if (cameraConfinerManager != null && currentConfiner != null)
        {
            cameraConfinerManager.SetConfiner(currentConfiner);
        }

        return currentRespawnPoint != null ? currentRespawnPoint : defaultRespawnPoint;
    }

    public string GetOverrideSortingLayer()
    {
        if (currentCheckpoint != null &&
            currentCheckpoint.overrideRespawnSortingLayer)
        {
            return currentCheckpoint.respawnSortingLayer;
        }

        return null;
    }

    public int GetOverrideOrderInLayer()
    {
        if (currentCheckpoint != null &&
            currentCheckpoint.overrideRespawnSortingLayer)
        {
            return currentCheckpoint.respawnOrderInLayer;
        }

        return int.MinValue;
    }
}