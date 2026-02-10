using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    private bool activated = false;
    public bool makeSound = true;

    [Header("Modifiche al Mondo (Opzionale)")]
    public GameObject objectToHide; // Trascina qui l'Acqua_Fantasy
    public GameObject objectToShow; // Trascina qui la Tilemap_PonteMagico

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        // 1. Suono
        if (makeSound)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.checkpointSound);

        activated = true;

        // 2. Logica del Ponte Magico (Se ci sono oggetti assegnati)
        if (objectToHide != null)
        {
            objectToHide.SetActive(false); // Nasconde l'acqua
        }

        if (objectToShow != null)
        {
            objectToShow.SetActive(true); // Mostra il ponte e le particelle
        }

        // 3. Salva la posizione
        RespawnManager.Instance.SetCheckpoint(transform);
    }
}