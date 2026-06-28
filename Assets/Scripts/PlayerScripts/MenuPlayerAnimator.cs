using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MenuPlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private float nextBlinkTime;

    private void Start()
    {
        animator = GetComponent<Animator>();
        ImpostaProssimoBlink();
    }

    private void Update()
    {
        // Se è passato abbastanza tempo, fai il blink e ricalcola il timer
        if (Time.time >= nextBlinkTime)
        {
            animator.SetTrigger("Blink");
            ImpostaProssimoBlink();
        }
    }

    private void ImpostaProssimoBlink()
    {
        // Imposta un timer casuale tra 3 e 6 secondi per dare un effetto naturale
        nextBlinkTime = Time.time + Random.Range(3f, 6f);
    }
}