using UnityEngine;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    public ScreenFade screenFade;
    public SceneController sceneController;

    public Vector3 respawnPoint;
    public float respawnDelay = 1.5f;
    
    public GameObject deathParticle;
    public GameObject fullSprite;      
    public GameObject riggedBody;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;

    private bool isDying = false;


    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDying && collision.gameObject.CompareTag("Enemy"))
        {
            StartCoroutine(DeathSequence());
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDying && other.gameObject.CompareTag("Death"))
        {
            StartCoroutine(DeathSequence());
        }
    }


    private IEnumerator DeathSequence()
    {
        Debug.Log(">>> DeathSequence STARTED");

        isDying = true;

        // Blocca movimento e collisioni
        rb.linearVelocity = Vector2.zero;
        col.enabled = false;

        var movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        // Particelle
        if (deathParticle != null)
        {
            GameObject p = Instantiate(deathParticle, transform.position, Quaternion.identity);
            Debug.Log(">>> Death particle should have spawned now.");
            p.name = "DeathParticle_Instance"; // rende più facile trovarla nella Hierarchy

            // Forza trasformazioni "pulite"
            p.transform.position = transform.position;
            p.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            p.transform.localScale = Vector3.one;

            // Prendi il sistema particelle (root o child)
            ParticleSystem ps = p.GetComponent<ParticleSystem>() ?? p.GetComponentInChildren<ParticleSystem>();
            if (ps == null)
            {
                Debug.LogWarning("[Respawn] Nessun ParticleSystem trovato in prefab deathParticle!");
            }
            else
            {
                // Forza alcuni settings utili a debug/compatibilità
                var main = ps.main;
                main.simulationSpace = ParticleSystemSimulationSpace.World; // preferibile per effetti spawnati
                main.useUnscaledTime = true; // IMPORTANTE: usa tempo non scalato

                // Forza renderer sorting (metti il nome del tuo sorting layer se ne usi uno)
                var prs = ps.GetComponent<ParticleSystemRenderer>();
                if (prs != null)
                {
                    prs.sortingLayerName = "Default"; // prova anche "Foreground" o il layer del player
                    prs.sortingOrder = 100; // molto alto per stare davanti
                }

                Debug.Log($"[Respawn] Spawn particle @ {p.transform.position}. active={p.activeSelf}. isPlaying={ps.isPlaying}");

                // Clear + Simulate + Play per essere sicuri che parta subito
                ps.Clear(true);
                ps.Simulate(0f, true, true); // inizializza la simulazione
                ps.Play(true);

                Debug.Log($"[Respawn] Dopo Play: isPlaying={ps.isPlaying}, particleCount={ps.particleCount}");
            }
        }
        /*if (deathParticle != null)
        {
            var p = Instantiate(deathParticle, transform.position, Quaternion.identity);

            var ps = p.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play(true); // forza l'avvio anche con timeScale = 0
            }
        }*/

        if (fullSprite != null)
        {
            var sr = fullSprite.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = false;
        }

        if (riggedBody != null)
        {
            // Mantieni pos/rot/scale del player sul rig per evitare "salti"
            riggedBody.transform.position = transform.position;
            riggedBody.transform.rotation = transform.rotation;
            riggedBody.transform.localScale = transform.localScale;

            riggedBody.SetActive(true);
        }

        // Animazione morte
        if (animator != null)
            animator.SetTrigger("Die");


        // Aspetta animazione
        yield return new WaitForSeconds(respawnDelay);

        //FADE OUT (schermo si scurisce) 
        if (screenFade != null)
        {
            yield return screenFade.FadeOutCoroutine(sceneController.fadeDuration);
        }


        // Respawn
        transform.position = respawnPoint;

        // prima del respawn, resetta oggetti
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.ResetAll();

        if (riggedBody != null) riggedBody.SetActive(false);
        if (fullSprite != null)
        {
            var sr = fullSprite.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;
        }

        if (animator != null)
             animator.SetTrigger("Respawn");

        if (movement != null) movement.enabled = true;
        if (col != null) col.enabled = true;


        //FADE IN (torna visibile)
        if (screenFade != null)
        {
            yield return screenFade.FadeInCoroutine(sceneController.fadeDuration);
        }

        isDying = false;
    }


    public bool IsDying() { return isDying; }
}
