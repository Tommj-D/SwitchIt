using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    private bool activated = false;
    public bool makeSound = true;

    [Header("Camera Settings")]
    [Tooltip("Inserisci qui il Collider2D del recinto in cui si trova questo checkpoint")]
    public Collider2D confinerDiQuestoCheckpoint;

    [Header("Respawn Sorting Layer")]
    public bool overrideRespawnSortingLayer = false;
    public string respawnSortingLayer = "Default";
    public int respawnOrderInLayer = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        PlayerRespawn respawn = other.GetComponent<PlayerRespawn>();

        if (respawn == null)
            return;

        if (respawn.IsDying())
            return;

        if (makeSound)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.checkpointSound);

        activated = true;

        // Ora passiamo al RespawnManager sia la posizione che il recinto della telecamera!
        RespawnManager.Instance.SetCheckpoint(transform, confinerDiQuestoCheckpoint);
    }
}