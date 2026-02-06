using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    private bool activated = false;
    public bool makeSound = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        if (makeSound)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.checkpointSound);

        activated = true;

        RespawnManager.Instance.SetCheckpoint(transform);
    }
}
