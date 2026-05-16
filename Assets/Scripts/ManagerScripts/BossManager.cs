using UnityEngine;
using System.Collections;

public class BossManager : MonoBehaviour
{
    [Header("Fase 1: Pattugliamento (Piano 1)")]
    [Tooltip("Trascina qui i punti in cui si muoverà all'inizio")]
    public Transform[] puntiPattugliaFase1; 
    public Vector3 rotazioneAlPunto = new Vector3(0f, 180f, 0f); // Per farlo voltare

    [Header("Fasi Successive")]
    [Tooltip("Punto in cui scappa al Piano 2 (dopo la 1° hit)")]
    public Transform puntoFase2; 
    [Tooltip("Punto in cui scappa al Piano 3 (dopo la 2° hit)")]
    public Transform puntoFase3; 

    [Header("Pattugliamento Fase 2")]
    public Transform[] puntiPattugliaFase2;

    [Header("Pattugliamento Fase 3")]
    public Transform[] puntiPattugliaFase3;
    
    public float velocitaSpostamento = 5f;

    [Header("Spawner Fase 2 (Nemici Bianchi)")]
    public ContinuousSlimeSpawner[] spawnerFaseDue; 

    [Header("Effetti Audio e Visivi")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip idleSound; 
    [SerializeField] private AudioClip hitSound;  
    [SerializeField] private AudioClip deathSound; 
    [SerializeField] private ParticleSystem deathParticle;

    [SerializeField] private ParticleSystem teleportDisappearParticle;
    [SerializeField] private ParticleSystem teleportAppearParticle;

    [Header("Tempi Teletrasporto")]
    [SerializeField] private float tempoPrimaScomparsa = 0.4f;
    [SerializeField] private float tempoPrimaRicomparsa = 1f;

    [Header("Statistiche")]
    private int hp = 3;
    private bool isInvulnerable = false;
    private bool isDead = false;

    // Variabili interne
    private Animator anim;
    private Vector3 targetPos;
    private int indicePuntoAttuale = 0;
    private int faseAttuale = 1; // Controlla in che fase siamo (1, 2 o 3)

    private float velocitaOriginale;

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

        velocitaOriginale = velocitaSpostamento;
    }

    private void PlayIdleSound()
    {
        if (!isDead && audioSource != null && idleSound != null)
        {
            audioSource.PlayOneShot(idleSound);
        }
    }

    private Transform[] GetPuntiFaseCorrente()
    {
        switch (faseAttuale)
        {
            case 1:
                return puntiPattugliaFase1;

            case 2:
                return puntiPattugliaFase2;

            case 3:
                return puntiPattugliaFase3;
        }

        return puntiPattugliaFase1;
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        Transform[] puntiCorrenti = GetPuntiFaseCorrente();

        if (puntiCorrenti != null && puntiCorrenti.Length > 0)
        {
            if (Vector2.Distance(transform.position, targetPos) < 0.2f)
            {
                transform.eulerAngles += rotazioneAlPunto;

                indicePuntoAttuale++;

                if (indicePuntoAttuale >= puntiCorrenti.Length)
                {
                    indicePuntoAttuale = 0;
                }

                targetPos = puntiCorrenti[indicePuntoAttuale].position;
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
        isInvulnerable = true;

        velocitaSpostamento = 0f;

        if (anim != null)
            anim.SetTrigger("Hit");

        yield return new WaitForSeconds(tempoPrimaScomparsa);

        // PARTICELLE SPARIZIONE
        if (teleportDisappearParticle != null)
        {
            Instantiate(
                teleportDisappearParticle,
                transform.position,
                Quaternion.identity
            ).Play();
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null)
            sr.enabled = false;

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();

        foreach (Collider2D c in colliders)
            c.enabled = false;

        faseAttuale = nuovaFase;

        indicePuntoAttuale = 0;

        // TELETRASPORTO
        if (faseAttuale == 2)
        {
            if (puntoFase2 != null)
            {
                transform.position = puntoFase2.position;
                transform.rotation = Quaternion.identity;
            }

            if (puntiPattugliaFase2.Length > 0)
            {
                targetPos = puntiPattugliaFase2[0].position;
            }

            foreach (var spawner in spawnerFaseDue)
            {
                if (spawner != null)
                    spawner.gameObject.SetActive(true);
            }
        }
        else if (faseAttuale == 3)
        {
            if (puntoFase3 != null)
            {
                transform.position = puntoFase3.position;
                transform.rotation = Quaternion.identity;
            }

            if (puntiPattugliaFase3.Length > 0)
            {
                targetPos = puntiPattugliaFase3[0].position;
            }

            foreach (var spawner in spawnerFaseDue)
            {
                if (spawner != null)
                    spawner.gameObject.SetActive(false);
            }
        }

        // TEMPO NASCOSTO
        yield return new WaitForSeconds(tempoPrimaRicomparsa);

        // PARTICELLE RICOMPARSA
        if (teleportAppearParticle != null)
        {
            Instantiate(
                teleportAppearParticle,
                transform.position,
                Quaternion.identity
            ).Play();
        }

        // RICOMPARSA
        if (sr != null)
            sr.enabled = true;

        foreach (Collider2D c in colliders)
            c.enabled = true;

        velocitaSpostamento = velocitaOriginale;

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