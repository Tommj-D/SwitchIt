using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    public Transform runeSpawnPoint;
    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        activated = true;

        AudioManager.instance.PlaySFX(AudioManager.instance.checkpointSound);

        PlayerRespawn playerRespawn = other.GetComponent<PlayerRespawn>();
        if (playerRespawn != null)
        {
            playerRespawn.SetRespawnPoint(runeSpawnPoint);
        }
    }
}
