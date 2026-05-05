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

    [Header("Particelle verso target")]
    public GameObject particlePrefab;
    public float particleSpeed = 5f;
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
        if (slimePrefab == null || particlePrefab == null)
        {
            Debug.LogWarning("Prefab mancanti!");
            return;
        }

        Vector3 baseSpawnPos = spawnCenter != null ? spawnCenter.position : transform.position;

        // 👉 UN SOLO punto di arrivo (fondamentale)
        float randomX = Random.Range(-spawnRadius, spawnRadius);
        Vector3 targetPos = baseSpawnPos + new Vector3(randomX, 0.5f, 0f);

        for (int i = 0; i < slimesPerPress; i++)
        {
            GameObject particle = Instantiate(particlePrefab, transform.position, Quaternion.identity);

            SimpleParticleToTarget p = particle.GetComponent<SimpleParticleToTarget>();
            p.speed = particleSpeed;

            if (i == 0)
            {
                p.Init(targetPos, () =>
                {
                    GameObject slime = Instantiate(slimePrefab, targetPos, Quaternion.identity);
                    spawnedSlimes.Add(slime);
                });
            }
            else
            {
                p.Init(targetPos, null);
            }
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