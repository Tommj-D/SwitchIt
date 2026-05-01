using System.Collections;
using UnityEngine;

public class SlimeSpawnerButton : MonoBehaviour
{
    [Header("Impostazioni Slime")]
    public GameObject slimePrefab; // Trascina qui il Prefab del tuo Slime
    public int slimesPerPress = 2; // Quanti slime compaiono ogni volta che premi la roccia
    public float spawnRadius = 2f; // La distanza massima (a destra e sinistra) in cui possono spawnare
    public Transform spawnCenter;  // (Opzionale) Se vuoi che compaiano in un punto esatto, assegna un Transform qui. Altrimenti userà la posizione della roccia.

    [Header("Impostazioni Pulsante")]
    public float cooldownDelay = 1.5f; // Secondi da aspettare prima di poter ripremere la roccia
    private float lastPressTime = -100f; // Memoria interna per il cooldown

    [Header("Effetti")]
    public bool isAnimated = true;
    public ParticleSystem particellePolvere;
    private Animator animator;

    private bool canPress = true;

    private void Start()
    {
        if (isAnimated)
            animator = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Controlliamo che sia il giocatore
        if (!other.CompareTag("Player")) return;

        if (!canPress) return;

        // 2. Controlliamo se il pulsante è ancora in "Ricarica" (Cooldown)
        canPress = false;

        // Aggiorniamo il tempo dell'ultima pressione
        lastPressTime = Time.time;

        // 3. Facciamo comparire gli slime!
        SpawnSlimes();

        // 4. Avviamo gli effetti visivi e sonori
        PlayEffects();

        StartCoroutine(ResetButton());
    }

    private void SpawnSlimes()
    {
        if (slimePrefab == null)
        {
            Debug.LogWarning("Attenzione: Nessun prefeb Slime assegnato al pulsante!");
            return;
        }

        // Scegliamo da dove far partire gli slime (se hai messo un centro personalizzato usa quello, sennò usa la roccia)
        Vector3 baseSpawnPos = spawnCenter != null ? spawnCenter.position : transform.position;

        for (int i = 0; i < slimesPerPress; i++)
        {
            // Calcoliamo una posizione casuale a destra o a sinistra della roccia
            float randomX = Random.Range(-spawnRadius, spawnRadius);
            
            // Aggiungiamo un pizzico di Y (es. 0.5f) per essere sicuri che lo slime non nasca incastrato nel pavimento
            Vector3 finalSpawnPos = baseSpawnPos + new Vector3(randomX, 0.5f, 0f);

            // Creiamo fisicamente lo slime
            Instantiate(slimePrefab, finalSpawnPos, Quaternion.identity);
        }
    }

    private void PlayEffects()
    {
        if (animator != null && isAnimated)
        {
            animator.SetTrigger("Press");
        }

        if (AudioManager.Instance != null && AudioManager.Instance.sfxSource != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buttonSound);
        }

        if (particellePolvere != null)
        {
            particellePolvere.Play();
        }
    }

    private IEnumerator ResetButton()
    {
        yield return new WaitForSeconds(cooldownDelay);

        if (animator != null && isAnimated)
        {
            animator.SetTrigger("Reset"); // 👈 trigger di ritorno
        }

        // stesso suono della pressione
        if (AudioManager.Instance != null && AudioManager.Instance.sfxSource != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buttonSound);
        }

        if (particellePolvere != null)
        {
            particellePolvere.Play();
        }
    }

    public void OnResetAnimationFinished()
    {
        canPress = true;

        if (animator != null && isAnimated)
        {
            animator.SetTrigger("Idle"); //torna all'idle
        }
    }

}