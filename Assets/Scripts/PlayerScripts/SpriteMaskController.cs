using UnityEngine;

public class SpriteMaskController : MonoBehaviour
{
    [SerializeField] private UnityEngine.SpriteMask spriteMask;

    private void Start()
    {
        if (spriteMask != null)
            spriteMask.enabled = false; // disabilita la maschera all'avvio
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hidden") && spriteMask != null)
            spriteMask.enabled = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Hidden") && spriteMask != null)
            spriteMask.enabled = false;
    }
}
