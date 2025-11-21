using UnityEngine;

[DisallowMultipleComponent]
public class RespawnableObject : MonoBehaviour
{
    // stato iniziale
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 startScale;
    private bool startActive;

    // RB/Collider/Animator (opzionali)
    private Rigidbody2D rb2d;
    private Vector2 startLinearVelocity;
    private float startAngularVelocity;
    private bool hadRigidbody;
    private bool startSimulated;
    private Collider2D[] colliders;

    private Animator animator;
    private RuntimeAnimatorController startController;
    private int startAnimatorStateHash = -1;

    private void Awake()
    {
        // salva trasform
        startPosition = transform.position;
        startRotation = transform.rotation;
        startScale = transform.localScale;
        startActive = gameObject.activeSelf;

        // rigidbody (se presente)
        rb2d = GetComponent<Rigidbody2D>();
        hadRigidbody = rb2d != null;
        if (hadRigidbody)
        {
            startLinearVelocity = rb2d.linearVelocity;
            startAngularVelocity = rb2d.angularVelocity;
            startSimulated = rb2d.simulated;
        }

        // colliders per toggle
        colliders = GetComponents<Collider2D>();

        // animator (se presente) - salva controller / stato
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            startController = animator.runtimeAnimatorController;
            // opzionale: salva stato corrente
            var state = animator.GetCurrentAnimatorStateInfo(0);
            startAnimatorStateHash = state.fullPathHash;
        }

        // registrati al manager (opzionale se vuoi autocollect)
        RespawnManager.Register(this);
    }

    private void OnDestroy()
    {
        RespawnManager.Unregister(this);
    }

    // chiamare per resettare allo stato iniziale
    public void ResetToStart()
    {
        // riattiva/spegni GameObject come all'inizio
        gameObject.SetActive(startActive);

        // ripristina transform
        transform.position = startPosition;
        transform.rotation = startRotation;
        transform.localScale = startScale;

        // rigidbody
        if (hadRigidbody && rb2d != null)
        {
            rb2d.linearVelocity = startLinearVelocity;
            rb2d.angularVelocity = startAngularVelocity;
            rb2d.simulated = startSimulated;
            rb2d.Sleep(); // evita scatti immediati
        }

        // colliders
        if (colliders != null)
            foreach (var c in colliders)
                if (c != null) c.enabled = true;

        // animator reset (se presente)
        if (animator != null)
        {
            animator.runtimeAnimatorController = startController;
            // forza stato di partenza (se valido)
            if (startAnimatorStateHash != -1)
            {
                animator.Play(startAnimatorStateHash, 0, 0f);
                animator.Update(0f);
            }
        }
    }
}

