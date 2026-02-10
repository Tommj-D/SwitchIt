using UnityEngine;

public class MagicTeleport : MonoBehaviour
{
    public Transform destinazione; // Dove andiamo?

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Teletrasporta il giocatore alla posizione della destinazione
            collision.transform.position = destinazione.position;
        }
    }
}