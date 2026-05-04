using System.Collections;
using System.Collections.Generic; // Fondamentale per usare le List
using UnityEngine;

public class SlimeSpawnerButton : MonoBehaviour
{
    [Header("Impostazioni Slime")]
    public GameObject slimePrefab; 
    public int slimesPerPress = 2; 
    public float spawnRadius = 2f; 
    public Transform spawnCenter;  

    [Header("Impostazioni Pulsante")]
    public float cooldownDelay = 1.5f; 
    private float lastPressTime = -100f; 

    [Header("Effetti")]
    public bool isAnimated = true;
    public ParticleSystem particellePolvere;
    private Animator animator;

    private bool canPress = true;

    // NUOVO: La memoria degli slime creati da questo pulsante
    private List<GameObject> spawnedSlimes = new List<GameObject>();

    private void Start()
    {
        if (isAnimated)
            animator = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!canPress) return;

        canPress = false;
        lastPressTime = Time.time;

        SpawnSlimes();
        PlayEffects();

        StartCoroutine(ResetButton());
    }

    private void SpawnSlimes()
    {
        if (slimePrefab == null)
        {
            Debug.LogWarning("Attenzione: Nessun prefab Slime assegnato al pulsante!");
            return;
        }

        Vector3 baseSpawnPos = spawnCenter != null ? spawnCenter.position : transform.position;

        for (int i = 0; i < slimesPerPress; i++)
        {
            float randomX = Random.Range(-spawnRadius, spawnRadius);
            Vector3 finalSpawnPos = baseSpawnPos + new Vector3(randomX, 0.5f, 0f);

            // Salviamo il nuovo slime in una variabile temporanea...
            GameObject newSlime = Instantiate(slimePrefab, finalSpawnPos, Quaternion.identity);
            
            // ...e lo aggiungiamo alla nostra lista della memoria!
            spawnedSlimes.Add(newSlime);
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
            animator.SetTrigger("Reset"); 
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

    public void OnResetAnimationFinished()
    {
        canPress = true;

        if (animator != null && isAnimated)
        {
            animator.SetTrigger("Idle"); 
        }
    }

    // ==========================================
    // NUOVA FUNZIONE: Da chiamare quando il giocatore muore
    // ==========================================
    public void ResetSpawner()
    {
        // 1. Ferma tutto se il bottone era a metà animazione/cooldown
        StopAllCoroutines();
        canPress = true;

        // 2. Forza l'animazione a tornare normale (se c'è)
        if (animator != null && isAnimated)
        {
            animator.Play("Idle"); // Usa Play per forzarlo istantaneamente
        }

        // 3. Distrugge tutti gli slime nati da questa roccia
        foreach (GameObject slime in spawnedSlimes)
        {
            if (slime != null)
            {
                Destroy(slime);
            }
        }
        
        // 4. Pulisce la memoria per il prossimo tentativo
        spawnedSlimes.Clear();
    }
}