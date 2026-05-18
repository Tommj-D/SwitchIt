using UnityEngine;
using System.Collections;

public class BossManager : MonoBehaviour
{
    //==================================================
    // 🎁 REWARD / PARTICELLE PROGRESSIONE
    //==================================================
    [Header("Reward Particles")]
    [SerializeField] private ParticleSystem flyingRewardParticlePrefab;
    [SerializeField] private BossRewardSpawner firstHitRewards;
    [SerializeField] private BossRewardSpawner secondHitRewards;

    //==================================================
    // 🧭 PERCORSI FASI BOSS
    //==================================================
    [Header("Fase 1: Pattugliamento")]
    public Transform[] puntiPattugliaFase1;
    public Vector3 rotazioneAlPunto = new Vector3(0f, 180f, 0f);

    [Header("Fase 2 / 3 - Teleport Points")]
    public Transform puntoFase2;
    public Transform puntoFase3;

    [Header("Pattugliamento Fase 2")]
    public Transform[] puntiPattugliaFase2;

    [Header("Pattugliamento Fase 3")]
    public Transform[] puntiPattugliaFase3;

    public float velocitaSpostamento = 5f;

    [SerializeField] private Vector2 turnCheckOffset = new Vector2(0f, -1.5f);

    //==================================================
    // 👾 SPAWN MINION SYSTEM
    //==================================================
    [Header("Spawn Minions")]
    [SerializeField] private GameObject minionPrefab;
    [Header("Spawn Minions via Particles")]
    [SerializeField] private FlyingMinionParticle flyingMinionPrefab;
    [SerializeField] private Transform puntoMinionSinistra;
    [SerializeField] private Transform puntoMinionDestra;

    [Header("Infinite Minion Spawn")]
    [SerializeField] private Transform spawnLoopSinistra;
    [SerializeField] private Transform spawnLoopDestra;
    [SerializeField] private float spawnLoopInterval = 3f;

    private Coroutine spawnLoopCoroutine;

    [Header("Portal FX")]
    [SerializeField] private ParticleSystem portalFXPrefab;

    private ParticleSystem portalLeftInstance;
    private ParticleSystem portalRightInstance;

    //==================================================
    // ⚡ DASH FASE 3
    //==================================================
    [Header("Dash Fase 3")]
    [SerializeField] private bool usaDashFase3 = true;
    [SerializeField] private float tempoAttesaDash = 2f;
    [SerializeField] private float velocitaDash = 20f;
    [SerializeField] private float durataDash = 0.35f;

    private bool isDashing = false;
    private Coroutine dashRoutine;

    //==================================================
    // 🎧 AUDIO + FX
    //==================================================
    [Header("Audio & VFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip idleSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    
    [Tooltip("Il ruggito che fa il boss prima di iniziare a muoversi")]
    [SerializeField] private AudioClip roarSound; // NUOVO

    [SerializeField] private ParticleSystem deathParticle;
    [SerializeField] private ParticleSystem teleportDisappearParticle;
    [SerializeField] private ParticleSystem teleportAppearParticle;

    //==================================================
    // 💥 HIT SHOCKWAVE
    //==================================================
    [Header("Hit ShockWave")]
    [SerializeField] private bool useHitShockWave = true;
    [SerializeField] private float hitShockWaveDuration = 1f;
    [Range(-5f, 5f)]
    [SerializeField] private float hitShockWaveStrength = -0.1f;
    [SerializeField] private float hitShockWaveXSizeRatio = 1f;
    
    //==================================================
    // ⏱ TIMING TELEPORT
    //==================================================
    [Header("Teleport Timing")]
    [SerializeField] private float tempoPrimaScomparsa = 0.4f;
    [SerializeField] private float tempoPrimaRicomparsa = 1f;

    //==================================================
    // 🌫 DISSOLVE SHADER
    //==================================================
    [Header("Dissolve Shader")]
    [SerializeField] private float dissolveTime = 0.5f;
    [SerializeField] private bool useVerticalDissolve = false;

    [Header("Shader Settings")]
    [SerializeField] private float outlineThickness = 0.1f;
    [SerializeField] private float dissolveScale = 30f;

    [ColorUsage(true, true)]
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float spiralStrength = 5f;

    private Material[] dissolveMaterials;

    private int dissolveAmountID = Shader.PropertyToID("_DissolveAmount");
    private int verticalDissolveID = Shader.PropertyToID("_VerticalDissolve");
    private int outlineThicknessID = Shader.PropertyToID("_OutlineThickness");
    private int outlineColorID = Shader.PropertyToID("_OutlineColor");
    private int spiralStrengthID = Shader.PropertyToID("_SpiralStrength");
    private int dissolveScaleID = Shader.PropertyToID("_DissolveScale");

    //==================================================
    // ❤️ STATS BOSS
    //==================================================
    [Header("Knockback Settings")]
    public float forzaKnockback = 10f; 
    private int hp = 3;
    private bool isInvulnerable = false;
    private bool isDead = false;

    //==================================================
    // 🧠 STATE MACHINE INTERNA
    //==================================================
    private Animator anim;
    private Rigidbody2D rb;

    private Vector3 targetPos;
    private int indicePuntoAttuale = 0;
    private int faseAttuale = 1;

    private int hitIndex = 0;
    private bool isTransitioning = false;
    private float velocitaOriginale;

    // --- NUOVE VARIABILI REGIA ---
    private bool isFightStarted = false; // Ferma il boss finché non urla
    private Vector3 posizioneDiPartenza; // Dove riposizionarlo se muori

    //==================================================
    // START / AWAKE
    //==================================================
    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        SetupDissolveMaterials();
    }

    private void Start()
    {
        posizioneDiPartenza = transform.position; // Salva la posizione iniziale

        if (puntiPattugliaFase1.Length > 0 && puntiPattugliaFase1[0] != null)
        {
            targetPos = puntiPattugliaFase1[0].position;
        }

        if (idleSound != null)
            InvokeRepeating(nameof(PlayIdleSound), 1f, 8f);

        velocitaOriginale = velocitaSpostamento;
    }

    //==================================================
    // 🎬 METODI PER LA REGIA E L'INTRO (NUOVO)
    //==================================================
    public float EmettiRuggito()
    {
        // Se hai un'animazione di ruggito, puoi scommentare qui:
        // if (anim != null) anim.SetTrigger("Roar"); 

        if (audioSource != null && roarSound != null)
        {
            audioSource.PlayOneShot(roarSound);
            return roarSound.length; // Calcola quanto dura l'urlo
        }
        return 1.5f; // Fallback di 1.5 secondi se manca il file audio
    }

    public void IniziaCombattimento()
    {
        isFightStarted = true; // Dà il via libera al movimento!
    }

    public void ResetInizio()
    {
        isFightStarted = false; // Blocca il boss
        transform.position = posizioneDiPartenza; // Lo rimette al suo posto
        
        // Lo fa ri-puntare al primo punto di pattuglia per la prossima volta
        if (puntiPattugliaFase1.Length > 0 && puntiPattugliaFase1[0] != null)
        {
            targetPos = puntiPattugliaFase1[0].position;
            indicePuntoAttuale = 0;
        }
    }

    //==================================================
    // AUDIO LOOP
    //==================================================
    private void PlayIdleSound()
    {
        // Il suono in idle ora parte solo SE la battaglia è iniziata
        if (!isDead && isFightStarted && audioSource != null && idleSound != null)
            audioSource.PlayOneShot(idleSound);
    }

    //==================================================
    // MOVIMENTO BOSS
    //==================================================
    private Transform[] GetPuntiFaseCorrente()
    {
        switch (faseAttuale)
        {
            case 2: return puntiPattugliaFase2;
            case 3: return puntiPattugliaFase3;
            default: return puntiPattugliaFase1;
        }
    }

    private void FixedUpdate()
    {
        // SE LA FIGHT NON È INIZIATA, IL BOSS STA FERMO!
        if (isDead || isTransitioning || isDashing || !isFightStarted) return;

        Transform[] punti = GetPuntiFaseCorrente();

        float y = rb != null ? rb.position.y : transform.position.y;
        Vector2 target = new Vector2(targetPos.x, y);

        if (punti != null && punti.Length > 0)
        {
            Vector2 check = (Vector2)transform.position + turnCheckOffset;
            Vector2 targetCheck = target + turnCheckOffset;

            if (Vector2.Distance(check, targetCheck) < 0.2f)
            {
                transform.eulerAngles += rotazioneAlPunto;

                indicePuntoAttuale++;
                if (indicePuntoAttuale >= punti.Length)
                    indicePuntoAttuale = 0;

                targetPos = punti[indicePuntoAttuale].position;
            }
        }

        if (rb != null)
        {
            Vector2 newPos = Vector2.MoveTowards(rb.position, target, velocitaSpostamento * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, target, velocitaSpostamento * Time.fixedDeltaTime);
        }
    }

    //==================================================
    // DAMAGE / HIT SYSTEM
    //==================================================
    public void PrendiDanno(GameObject player) 
    {
        if (isInvulnerable || hp <= 0 || isDead || isTransitioning || !isFightStarted) return;

        hp--;
        isInvulnerable = true;

        if (anim != null) anim.SetTrigger("Hit");
        if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);
        
         if (useHitShockWave && ShockWaveManager.Instance != null)
        {
            ShockWaveManager.Instance.SetXSizeRatio(hitShockWaveXSizeRatio);

            ShockWaveManager.Instance.CallShockWave(
                transform.position,
                hitShockWaveStrength,
                hitShockWaveDuration
            );
        }

        // --- INIZIO LOGICA KNOCKBACK ---
        if (player != null)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 direzioneSpinta = (player.transform.position - transform.position).normalized;
                direzioneSpinta.y = 0.5f; 

                playerRb.linearVelocity = Vector2.zero; 
                playerRb.AddForce(direzioneSpinta * forzaKnockback, ForceMode2D.Impulse);
            }
        }
        // --- FINE LOGICA KNOCKBACK ---

        if (hp <= 0)
        {
            Muori();
            return; 
        }

        StartCoroutine(HitCycleRoutine());
    }

    private IEnumerator HitCycleRoutine()
    {
        isTransitioning = true; 

        Collider2D[] tuttiIColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in tuttiIColliders)
        {
            col.enabled = false;
        }

        yield return new WaitForSeconds(tempoPrimaScomparsa);

        if (teleportDisappearParticle != null)
            Instantiate(teleportDisappearParticle, transform.position, Quaternion.identity).Play();

        yield return StartCoroutine(DissolveRoutine(0f, 1.1f));

        SpawnMinionsHit();

        if (hitIndex == 0)
            SpawnRewardParticles(firstHitRewards);
        else
            SpawnRewardParticles(secondHitRewards);

        yield return new WaitForSeconds(tempoPrimaRicomparsa);

        if (hitIndex == 0) 
        {
            faseAttuale = 2;
            transform.position = puntoFase2.position;
            transform.rotation = puntoFase2.rotation; 
            
            if (puntiPattugliaFase2.Length > 0)
            {
                targetPos = puntiPattugliaFase2[0].position;
                indicePuntoAttuale = 0;
            }
        }
        else if (hitIndex == 1) 
        {
            faseAttuale = 3;
            transform.position = puntoFase3.position;
            transform.rotation = puntoFase3.rotation; 
            
            if (puntiPattugliaFase3.Length > 0)
            {
                targetPos = puntiPattugliaFase3[0].position;
                indicePuntoAttuale = 0;
            }
            if (spawnLoopCoroutine == null)
            {
                spawnLoopCoroutine = StartCoroutine(SpawnMinionLoopRoutine());
            }

            SpawnPortalFX();

            if (usaDashFase3 && dashRoutine == null)
            {
                dashRoutine = StartCoroutine(DashRoutine());
            }
        }

        if (teleportAppearParticle != null)
            Instantiate(teleportAppearParticle, transform.position, Quaternion.identity).Play();

        yield return StartCoroutine(DissolveRoutine(1.1f, 0f));

        SpawnMinionsHit();

        hitIndex++;

        foreach (Collider2D col in tuttiIColliders)
        {
            col.enabled = true;
        }

        isInvulnerable = false;
        isTransitioning = false;
    }

    //==================================================
    // SPAWN MINION
    //==================================================
    private void SpawnMinionsHit()
    {
        if (flyingMinionPrefab == null) return;

        if (puntoMinionSinistra != null)
        {
            FlyingMinionParticle particellaSx = Instantiate(flyingMinionPrefab, transform.position, Quaternion.identity);
            particellaSx.Setup(puntoMinionSinistra);
        }

        if (puntoMinionDestra != null)
        {
            FlyingMinionParticle particellaDx = Instantiate(flyingMinionPrefab, transform.position, Quaternion.identity);
            particellaDx.Setup(puntoMinionDestra);
        }
    }

    private IEnumerator SpawnMinionLoopRoutine()
    {
        while (!isDead)
        {
            SpawnLoopMinions();
            yield return new WaitForSeconds(spawnLoopInterval);
        }
    }

    private void SpawnLoopMinions()
    {
        if (minionPrefab == null) return;

        if (spawnLoopSinistra != null)
            Instantiate(minionPrefab, spawnLoopSinistra.position, Quaternion.identity);

        if (spawnLoopDestra != null)
            Instantiate(minionPrefab, spawnLoopDestra.position, Quaternion.identity);
    }

    private void SpawnPortalFX()
    {
        if (portalFXPrefab == null) return;

        if (spawnLoopSinistra != null)
        {
            portalLeftInstance = Instantiate(portalFXPrefab, spawnLoopSinistra.position, Quaternion.identity);
        }

        if (spawnLoopDestra != null)
        {
            portalRightInstance = Instantiate(portalFXPrefab, spawnLoopDestra.position, Quaternion.identity);
        }
    }

    private IEnumerator DashRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(tempoAttesaDash);

            if (isDead || isTransitioning)
                yield break;

            isDashing = true;

            float velocitaNormale = velocitaSpostamento;
            velocitaSpostamento = velocitaDash;

            yield return new WaitForSeconds(durataDash);

            velocitaSpostamento = velocitaNormale;

            isDashing = false;
        }
    }

    //==================================================
    // REWARD PARTICLE SYSTEM
    //==================================================
    private void SpawnRewardParticles(BossRewardSpawner rewards)
    {
        if (rewards == null) return;

        foreach (RewardTarget r in rewards.rewardTargets)
        {
            if (r == null) continue;

            ParticleSystem p = Instantiate(flyingRewardParticlePrefab, transform.position, Quaternion.identity);
            p.Play();

            FlyingRewardParticle mover = p.GetComponent<FlyingRewardParticle>();
            if (mover != null)
                mover.Setup(r.transform, r);
        }
    }

    //==================================================
    // DEATH
    //==================================================
    private void Muori()
    {
        isDead = true;

        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound);

        if (deathParticle != null)
            Instantiate(deathParticle, transform.position, transform.rotation).Play();

        Collider2D[] c = GetComponentsInChildren<Collider2D>();
        foreach (var col in c) col.enabled = false;

        GameObject[] minions = GameObject.FindGameObjectsWithTag("Minion");
        foreach (GameObject m in minions)
        {
            Destroy(m);
        }

        if (portalLeftInstance != null) Destroy(portalLeftInstance.gameObject);
        if (portalRightInstance != null) Destroy(portalRightInstance.gameObject);

        Destroy(gameObject, 3f);
    }

    //==================================================
    // 🌫 SHADER SYSTEM
    //==================================================
    private void SetupDissolveMaterials()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        dissolveMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            dissolveMaterials[i] = new Material(renderers[i].material);
            renderers[i].material = dissolveMaterials[i];
        }

        ApplyShaderSettings();
        SetDissolveAmount(0f);
    }

    private void ApplyShaderSettings()
    {
        if (dissolveMaterials == null) return;

        foreach (Material mat in dissolveMaterials)
        {
            if (mat.HasProperty(outlineThicknessID)) mat.SetFloat(outlineThicknessID, outlineThickness);
            if (mat.HasProperty(outlineColorID)) mat.SetColor(outlineColorID, outlineColor);
            if (mat.HasProperty(spiralStrengthID)) mat.SetFloat(spiralStrengthID, spiralStrength);
            if (mat.HasProperty(dissolveScaleID)) mat.SetFloat(dissolveScaleID, dissolveScale);
        }
    }

    private void SetDissolveAmount(float value)
    {
        if (dissolveMaterials == null) return;

        foreach (Material mat in dissolveMaterials)
        {
            if (mat.HasProperty(dissolveAmountID)) mat.SetFloat(dissolveAmountID, value);
            if (mat.HasProperty(verticalDissolveID)) mat.SetFloat(verticalDissolveID, useVerticalDissolve ? value : 0f);
        }
    }

    private IEnumerator DissolveRoutine(float start, float end)
    {
        float elapsed = 0f;
        while (elapsed < dissolveTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dissolveTime;
            float value = Mathf.Lerp(start, end, t);

            SetDissolveAmount(value);
            yield return null;
        }
        SetDissolveAmount(end);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + (Vector3)turnCheckOffset, 0.15f);
    }
}