using UnityEngine;
using System.Collections;

public class BossManager : MonoBehaviour
{
    [Header("Fase 1: Pattugliamento (Piano 1)")]
    [Tooltip("Trascina qui i 4 punti in cui si muoverà all'inizio")]
    public Transform[] puntiPattugliaFase1; 
    public Vector3 rotazioneAlPunto = new Vector3(0f, 180f, 0f); // Per farlo voltare

    [Header("Fasi Successive")]
    [Tooltip("Punto in cui scappa al Piano 2 (dopo la 1° hit)")]
    public Transform puntoFase2; 
    [Tooltip("Punto in cui scappa al Piano 3 (dopo la 2° hit)")]
    public Transform puntoFase3; 
    
    public float velocitaSpostamento = 5f;

    [Header("Spawner Fase 2 (Nemici Bianchi)")]
    public ContinuousSlimeSpawner[] spawnerFaseDue; 

    [Header("Effetti Audio e Visivi")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip idleSound; 
    [SerializeField] private AudioClip hitSound;  
    [SerializeField] private AudioClip deathSound; 
    [SerializeField] private ParticleSystem deathParticle;

    [Header("Statistiche")]
    private int hp = 3;
    private bool isInvulnerable = false;
    private bool isDead = false;

    // Variabili interne
    private Animator anim;
    private Vector3 targetPos;
    private int indicePuntoAttuale = 0;
    private int faseAttuale = 1; // Controlla in che fase siamo (1, 2 o 3)

    // Aggiungi questa riga in alto, vicino alle altre variabili "private"
    private Rigidbody2D rb;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        // Colleghiamo il Rigidbody all'avvio
        rb = GetComponent<Rigidbody2D>(); 
    }

    private void Start()
    {
        // Il boss parte immediatamente dal primo punto di pattuglia
        if (puntiPattugliaFase1.Length > 0 && puntiPattugliaFase1[0] != null)
        {
            targetPos = puntiPattugliaFase1[0].position;
            transform.position = targetPos;
        }

        if (idleSound != null)
        {
            InvokeRepeating("PlayIdleSound", 1f, 8f);
        }
    }

    private void PlayIdleSound()
    {
        if (!isDead && audioSource != null && idleSound != null)
        {
            audioSource.PlayOneShot(idleSound);
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        // LOGICA FASE 1 (Pattugliamento continuo sui 4 punti)
        if (faseAttuale == 1 && puntiPattugliaFase1.Length > 0)
        {
            if (Vector2.Distance(transform.position, targetPos) < 0.2f)
            {
                // Ruota il boss
                transform.eulerAngles += rotazioneAlPunto;
                
                // Passa al punto successivo
                indicePuntoAttuale++;
                if (indicePuntoAttuale >= puntiPattugliaFase1.Length) 
                {
                    indicePuntoAttuale = 0;
                }
                targetPos = puntiPattugliaFase1[indicePuntoAttuale].position;
            }
        }

        // MUOVE IL BOSS USANDO LA FISICA, ELIMINANDO GLI SCATTI
        if (rb != null)
        {
            Vector2 nuovaPos = Vector2.MoveTowards(rb.position, targetPos, velocitaSpostamento * Time.fixedDeltaTime);
            rb.MovePosition(nuovaPos);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, velocitaSpostamento * Time.fixedDeltaTime);
        }
    }

    public void PrendiDanno()
    {
        if (isInvulnerable || hp <= 0 || isDead) return;

        hp--;
        isInvulnerable = true;

        if (anim != null) anim.SetTrigger("Hit");
        
        if (audioSource != null && hitSound != null) 
            audioSource.PlayOneShot(hitSound);

        if (hp == 2)
        {
            StartCoroutine(CambioFase(2)); // Passa alla Fase 2
        }
        else if (hp == 1)
        {
            StartCoroutine(CambioFase(3)); // Passa alla Fase 3
        }
        else if (hp <= 0)
        {
            Muori();
        }
    }

    private IEnumerator CambioFase(int nuovaFase)
    {
        yield return new WaitForSeconds(0.5f);
        
        faseAttuale = nuovaFase;

        if (faseAttuale == 2) 
        {
            // Imposta la destinazione al Piano 2
            if (puntoFase2 != null) targetPos = puntoFase2.position;
            
            // Accende gli spawner
            foreach (var spawner in spawnerFaseDue)
            {
                if (spawner != null) spawner.gameObject.SetActive(true);
            }
        }
        else if (faseAttuale == 3) 
        {
            // Imposta la destinazione al Piano 3
            if (puntoFase3 != null) targetPos = puntoFase3.position;
            
            // Spegne gli spawner
            foreach (var spawner in spawnerFaseDue)
            {
                if (spawner != null) spawner.gameObject.SetActive(false);
            }
        }

        isInvulnerable = false;
    }

    private void Muori()
    {
        isDead = true;

        foreach (var spawner in spawnerFaseDue)
        {
            if (spawner != null) spawner.gameObject.SetActive(false);
        }

        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (anim != null) anim.SetTrigger("Die");
        
        if (deathParticle != null)
        {
            Instantiate(deathParticle, transform.position, transform.rotation).Play();
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D c in colliders) c.enabled = false;

        Debug.Log("IL BOSS È STATO SCONFITTO!");
        Destroy(gameObject, 3f);
    }
}