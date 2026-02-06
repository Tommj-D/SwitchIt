using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    private static List<RespawnableObject> allRespawnables = new List<RespawnableObject>();

    [Header("Player Respawn")]
    public Transform defaultRespawnPoint;
    private Transform currentRespawnPoint;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        currentRespawnPoint = defaultRespawnPoint;
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


