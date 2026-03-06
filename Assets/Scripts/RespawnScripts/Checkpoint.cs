using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    private bool activated = false;
    public bool makeSound = true;

    [Header("Camera Settings")]
    [Tooltip("Inserisci qui il Collider2D del recinto in cui si trova questo checkpoint")]
    public Collider2D confinerDiQuestoCheckpoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        if (makeSound)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.checkpointSound);

        activated = true;

        // Ora passiamo al RespawnManager sia la posizione che il recinto della telecamera!
        RespawnManager.Instance.SetCheckpoint(transform, confinerDiQuestoCheckpoint);
    }
}