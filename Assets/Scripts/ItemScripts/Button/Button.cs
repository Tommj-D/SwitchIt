using UnityEngine;

public class Button : MonoBehaviour
{
    public MonoBehaviour[] puzzleControllers;

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

        if (puzzleControllers == null || puzzleControllers.Length == 0)
        {
            Debug.LogWarning("Il Button non ha PuzzleController assegnati!");
            return;
        }

        activated = true;

        foreach (var controller in puzzleControllers)
        {
            if (controller is IButtonPuzzle puzzle)
            {
                puzzle.PressButton(transform, targetCircleIndex);
            }
        }

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

    public void ResetButton()
    {
        // Il funzionamento del puzzle (la logica) si resetta SEMPRE, 
        // sia che il pulsante sia attivo sia che sia disattivato.
        activated = false;

        // L'aspetto grafico (l'animazione) si resetta SUBITO solo se l'oggetto è attivo.
        // Se è disattivato, l'Animator è spento e Unity darebbe un errore.
        if (animator != null && isAnimated && gameObject.activeInHierarchy)
        {
            ResetAnimation();
        }
    }

    // Eseguiamo questo metodo nativo di Unity
    private void OnEnable()
    {
        // Ogni volta che il pulsante viene riattivato tramite SetActive(true),
        // se la logica era stata resettata (activated == false), allora forza
        // l'animazione a tornare allo stato originale (non premuto).
        if (!activated && animator != null && isAnimated)
        {
            ResetAnimation();
        }
    }

    // Un piccolo metodo di supporto per evitare di duplicare lo stesso codice
    private void ResetAnimation()
    {
        animator.Rebind();
        animator.Update(0f);
    }
}