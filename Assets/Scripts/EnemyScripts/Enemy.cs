using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Tooltip("Se true, il giocatore può uccidere il nemico saltandoci sopra.")]
    public bool isKillable = false;

    // Metodo chiamato quando il player stompa il nemico
    public virtual void OnStomp()
    {
        // Effetti, animazioni, disabilita collider ecc.
        Debug.Log($"{name} stomped!");
        Destroy(gameObject);
    }

    // Metodo chiamato quando il nemico colpisce il player lateralmente
    public virtual void OnHitPlayer()
    {
        // comportamento di default (puoi estenderlo)
        Debug.Log($"{name} hit the player!");
    }
}
