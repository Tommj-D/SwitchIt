using UnityEngine;

public class Button : MonoBehaviour
{
    private bool activated = false;
    private Animator animator;

    [Header("Oggetti da NASCONDERE")]
    public GameObject[] oggettiDaNascondere;

    [Header("Oggetti da MOSTRARE")]
    public GameObject[] oggettiDaMostrare;

    public bool isAnimated = true;

    private void Start()
    {
        if (isAnimated)
            animator = GetComponentInChildren<Animator>();

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

        if (animator != null && isAnimated)
        {
            animator.SetTrigger("Press");
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buttonSound);
        }
        else
        {
            AttivaOggetti();
        }
    }

    // Questa funzione verrà chiamata dall'animazione
    public void AttivaOggetti()
    {
        foreach (GameObject obj in oggettiDaNascondere)
        {
            if (obj != null) obj.SetActive(false);
        }

        foreach (GameObject obj in oggettiDaMostrare)
        {
            if (obj != null) obj.SetActive(true);
        }
    }
}