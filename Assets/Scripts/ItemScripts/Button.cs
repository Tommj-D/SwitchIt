using UnityEngine;

public class Button : MonoBehaviour
{
    private bool activated = false;
    [Header("Oggetti da NASCONDERE")]
    public GameObject[] oggettiDaNascondere;

    [Header("Oggetti da MOSTRARE")]
    public GameObject[] oggettiDaMostrare;

    private void Start()
    {
        // All'inizio disattiva tutti gli oggetti da mostrare
        foreach (GameObject obj in oggettiDaMostrare)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

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
    }
}
