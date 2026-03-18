using UnityEngine;

public class Button : MonoBehaviour
{
    public ButtonPuzzleController puzzleController;

    private bool activated = false;
    private Animator animator;

    [Header("Effetti")]
    public ParticleSystem particellePolvere;

    [Header("Target")]
    public int targetCircleIndex;

    public bool isAnimated = true;
    private void Start()
    {
        if (isAnimated)
            animator = GetComponentInChildren<Animator>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        if (puzzleController == null) 
        {
            Debug.LogWarning("Il Button non ha un riferimento al ButtonPuzzleController!");
            return;
        }

        activated = true;

        puzzleController.ButtonPressed(transform, targetCircleIndex);

        if (animator != null && isAnimated)
        {
            animator.SetTrigger("Press");

            if (AudioManager.Instance != null && AudioManager.Instance.sfxSource != null)
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buttonSound);

            if (particellePolvere != null)
            {
                particellePolvere.Play();
            }
        }
    }
}