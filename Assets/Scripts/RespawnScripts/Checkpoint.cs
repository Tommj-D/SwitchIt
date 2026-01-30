using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    public Transform runeSpawnPoint;
    private bool activated = false;
    public bool makeSound = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        if (makeSound)
            AudioManager.instance.PlaySFX(AudioManager.instance.checkpointSound);

        activated = true;

        PlayerRespawn playerRespawn = other.GetComponent<PlayerRespawn>();
        if (playerRespawn != null)
        {
            playerRespawn.SetRespawnPoint(runeSpawnPoint);
        }
    }
}
