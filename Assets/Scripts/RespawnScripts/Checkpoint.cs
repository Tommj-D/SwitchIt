using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    private bool activated = false;
    public bool makeSound = true;

    [Header("Oggetti da NASCONDERE")]
    public GameObject[] oggettiDaNascondere;

    [Header("Oggetti da MOSTRARE")]
    public GameObject[] oggettiDaMostrare;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        if (makeSound)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.checkpointSound);

        activated = true;

        // Spegne tutti gli oggetti nella lista "Nascondere"
        foreach (GameObject obj in oggettiDaNascondere)
        {
            if (obj != null) obj.SetActive(false);
        }

        // Accende tutti gli oggetti nella lista "Mostrare"
        foreach (GameObject obj in oggettiDaMostrare)
        {
            if (obj != null) obj.SetActive(true);
        }

        RespawnManager.Instance.SetCheckpoint(transform);
    }
}