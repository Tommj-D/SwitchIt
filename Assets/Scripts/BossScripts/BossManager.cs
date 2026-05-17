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
    [SerializeField] private Transform[] spawnPoints; // Usato per lo spawn generale se serve
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private bool spawnActive = false;
    [Header("Spawn Minions via Particles")]
    [SerializeField] private FlyingMinionParticle flyingMinionPrefab;
    [SerializeField] private Transform puntoMinionSinistra;
    [SerializeField] private Transform puntoMinionDestra;

    //==================================================
    // 🎧 AUDIO + FX
    //==================================================
    [Header("Audio & VFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip idleSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;

    [SerializeField] private ParticleSystem deathParticle;
    [SerializeField] private ParticleSystem teleportDisappearParticle;
    [SerializeField] private ParticleSystem teleportAppearParticle;

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
    public float forzaKnockback = 10f; // Quanto viene sbalzato lontano il player
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
        if (puntiPattugliaFase1.Length > 0)
        {
            targetPos = puntiPattugliaFase1[0].position;
            transform.position = targetPos;
        }

        if (idleSound != null)
            InvokeRepeating(nameof(PlayIdleSound), 1f, 8f);

        velocitaOriginale = velocitaSpostamento;
    }

    //==================================================
    // AUDIO LOOP
    //==================================================
    private void PlayIdleSound()
    {
        if (!isDead && audioSource != null && idleSound != null)
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
        // Se il boss è morto o si sta teletrasportando (isTransitioning), NON si muove.
        if (isDead || isTransitioning) return;

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
    // Ora passiamo il GameObject del player quando chiamiamo la funzione
    public void PrendiDanno(GameObject player) 
    {
        if (isInvulnerable || hp <= 0 || isDead || isTransitioning) return;

        hp--;
        isInvulnerable = true;

        if (anim != null) anim.SetTrigger("Hit");
        if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);

        // --- INIZIO LOGICA KNOCKBACK ---
        if (player != null)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Calcoliamo la direzione dal boss verso il player
                Vector2 direzioneSpinta = (player.transform.position - transform.position).normalized;
                
                // Aggiungiamo una piccola spinta verso l'alto per un effetto più bello
                direzioneSpinta.y = 0.5f; 

                // Azzeriamo la velocità attuale per un knockback più pulito, poi spingiamo
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

        // --- DISATTIVAZIONE DI TUTTI I COLLIDER (Anche della testa) ---
        Collider2D[] tuttiIColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in tuttiIColliders)
        {
            col.enabled = false;
        }

        // --- NUOVO: ATTESA PRIMA DI SCOMPARIRE ---
        // Usiamo la tua variabile per far aspettare il boss prima che si dissolva
        yield return new WaitForSeconds(tempoPrimaScomparsa);

        // Ora parte l'effetto visivo di scomparsa
        if (teleportDisappearParticle != null)
            Instantiate(teleportDisappearParticle, transform.position, Quaternion.identity).Play();

        yield return StartCoroutine(DissolveRoutine(0f, 1.1f));

        // Spawna i minion usando il nuovo sistema di particelle volanti
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
        }

        if (teleportAppearParticle != null)
            Instantiate(teleportAppearParticle, transform.position, Quaternion.identity).Play();

        yield return StartCoroutine(DissolveRoutine(1.1f, 0f));

        // Riattiviamo lo spawn anche al ritorno
        SpawnMinionsHit();

        hitIndex++;

        // --- RIATTIVAZIONE DI TUTTI I COLLIDER ---
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

        // Spawna la prima particella e dille di andare a sinistra
        if (puntoMinionSinistra != null)
        {
            FlyingMinionParticle particellaSx = Instantiate(flyingMinionPrefab, transform.position, Quaternion.identity);
            particellaSx.Setup(puntoMinionSinistra);
        }

        // Spawna la seconda particella e dille di andare a destra
        if (puntoMinionDestra != null)
        {
            FlyingMinionParticle particellaDx = Instantiate(flyingMinionPrefab, transform.position, Quaternion.identity);
            particellaDx.Setup(puntoMinionDestra);
        }
    }

    private void SpawnMinionsReturn()
    {
        // Spawna 2 minion (anziché 3) in posizioni casuali tra gli spawnPoints generali
        // (Se li vuoi sempre a destra e sinistra del boss anche qui, puoi richiamare SpawnMinionsHit!)
        for (int i = 0; i < 2; i++) SpawnRandomMinion();
    }

    private void SpawnRandomMinion()
    {
        if (minionPrefab == null || spawnPoints.Length == 0) return;

        Transform p = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(minionPrefab, p.position, Quaternion.identity);
    }

    private IEnumerator SpawnMinionsLoop()
    {
        while (spawnActive)
        {
            SpawnRandomMinion();
            yield return new WaitForSeconds(spawnInterval);
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

            // 1. Creiamo l'istanza della particella volante
            ParticleSystem p = Instantiate(flyingRewardParticlePrefab, transform.position, Quaternion.identity);
            
            // 2. FORZIAMO IL PLAY: Diciamo esplicitamente al Particle System di attivarsi e mostrare i dettagli visivi
            p.Play();

            // 3. Colleghiamo i componenti per il movimento verso l'obiettivo
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
            if (mat.HasProperty(outlineThicknessID))
                mat.SetFloat(outlineThicknessID, outlineThickness);

            if (mat.HasProperty(outlineColorID))
                mat.SetColor(outlineColorID, outlineColor);

            if (mat.HasProperty(spiralStrengthID))
                mat.SetFloat(spiralStrengthID, spiralStrength);

            if (mat.HasProperty(dissolveScaleID))
                mat.SetFloat(dissolveScaleID, dissolveScale);
        }
    }

    private void SetDissolveAmount(float value)
    {
        if (dissolveMaterials == null) return;

        foreach (Material mat in dissolveMaterials)
        {
            if (mat.HasProperty(dissolveAmountID))
                mat.SetFloat(dissolveAmountID, value);

            if (mat.HasProperty(verticalDissolveID))
                mat.SetFloat(verticalDissolveID, useVerticalDissolve ? value : 0f);
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