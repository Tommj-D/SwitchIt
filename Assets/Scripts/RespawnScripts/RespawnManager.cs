using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    private static List<RespawnableObject> allRespawnables = new List<RespawnableObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // API: chiamata dai RespawnableObject in Awake/OnDestroy
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
        // opzionale: puoi decidere ordine (prima oggetti statici, poi dinamici, ecc.)
        for (int i = 0; i < allRespawnables.Count; i++)
        {
            var r = allRespawnables[i];
            if (r != null) r.ResetToStart();
        }
    }
}

