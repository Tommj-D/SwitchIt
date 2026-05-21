using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps; 

public class BossCameraTrigger : MonoBehaviour
{
    [Header("Boss Camera")]
    [SerializeField] private CinemachineCamera bossCamera;

    [Header("Arena Walls")]
    [SerializeField] private GameObject muroReal;
    [SerializeField] private GameObject muroFantasy;
    
    [SerializeField] private float wallFadeDuration = 1f;

    // CAMBIATO: Da SpriteRenderer a Tilemap
    private Tilemap[] muroRealTilemaps;
    private Tilemap[] muroFantasyTilemaps;

    [Header("Boss")]
    [SerializeField] private BossManager bossManager;

    [Header("Audio")]
    [SerializeField] private BossAudioController bossAudio;

    [Header("Timing Cinematic")]
    [SerializeField] private float waitBeforeRoar = 1f;
    [SerializeField] private float waitAfterRoar = 0.5f;

    [Header("Player Walk (Time Based)")]
    [SerializeField] private float walkTime = 2f; 
    [SerializeField] private float walkSpeed = 4f;
    [Tooltip("1 per muoversi a destra, -1 per muoversi a sinistra")]
    [SerializeField] private float walkDirection = 1f; 

    private bool alreadyStarted;

    private void Start()
    {
        // Recuperiamo le Tilemap invece degli SpriteRenderer
        if (muroReal != null)
        {
            muroRealTilemaps = muroReal.GetComponentsInChildren<Tilemap>();
            SetTilemapsAlpha(muroRealTilemaps, 0f);
            muroReal.SetActive(false);
        }

        if (muroFantasy != null)
        {
            muroFantasyTilemaps = muroFantasy.GetComponentsInChildren<Tilemap>();
            SetTilemapsAlpha(muroFantasyTilemaps, 0f);
            muroFantasy.SetActive(false);
        }

        if (bossCamera != null)
            bossCamera.Priority = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (alreadyStarted)
            return;

        if (!collision.CompareTag("Player"))
            return;

        alreadyStarted = true;
        StartCoroutine(BossIntroRoutine(collision.gameObject));
    }

    private IEnumerator BossIntroRoutine(GameObject player)
    {
        //==================================================
        // PLAYER COMPONENTS
        //==================================================
        PlayerInput input = player.GetComponent<PlayerInput>();
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        //==================================================
        // BLOCCA CONTROLLI
        //==================================================
        if (input != null) input.enabled = false;
        if (movement != null) movement.enabled = false;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        //==================================================
        // ATTIVA CAMERA BOSS
        //==================================================
        if (bossCamera != null)
            bossCamera.Priority = 100;

        //==================================================
        // AUTO WALK A TEMPO
        //==================================================
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.enteringCaveFootstepSound);
        }

        float timer = 0f;

        while (timer < walkTime)
        {
            timer += Time.deltaTime;
            player.transform.position += new Vector3(walkDirection * walkSpeed * Time.deltaTime, 0f, 0f);
            yield return null;
        }

        //==================================================
        // STOP DEFINITIVO
        //==================================================
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Animator playerAnim = player.GetComponent<Animator>();
        if (playerAnim != null)
        {
            playerAnim.SetFloat("Speed", 0f); 
        }

        //==================================================
        // PAUSA PRE RUGGITO
        //==================================================
        yield return new WaitForSeconds(waitBeforeRoar);

        //==================================================
        // RUGGITO
        //==================================================
        float roarTime = 1.5f;
        if (bossManager != null)
        {
            roarTime = bossManager.EmettiRuggito();
        }

        //==================================================
        // CHIUSURA MURI CON FADE INTELLIGENTE
        //==================================================
        bool playerInFantasyWorld = false;
        if (WorldSwitch.Instance != null)
        {
            playerInFantasyWorld = WorldSwitch.Instance.isFantasyWorldActive;
        }

        if (muroReal != null)
        {
            muroReal.SetActive(true);
            if (!playerInFantasyWorld) 
            {
                // Se siamo nel mondo reale, fa il fade in
                StartCoroutine(FadeWall(muroRealTilemaps));
            }
            else 
            {
                // Altrimenti lo rende subito solido in background
                SetTilemapsAlpha(muroRealTilemaps, 1f);
            }
        }

        if (muroFantasy != null)
        {
            muroFantasy.SetActive(true);
            if (playerInFantasyWorld) 
            {
                // Se siamo nel mondo fantasy, fa il fade in
                StartCoroutine(FadeWall(muroFantasyTilemaps));
            }
            else 
            {
                // Altrimenti lo rende subito solido in background
                SetTilemapsAlpha(muroFantasyTilemaps, 1f);
            }
        }

        //==================================================
        // ASPETTA FINE RUGGITO
        //==================================================
        yield return new WaitForSeconds(roarTime + waitAfterRoar);

        //==================================================
        // BOSS MUSIC
        //==================================================
        if (bossAudio != null)
            bossAudio.StartBossMusic();

        if (VolumeController.Instance != null)
        {
            VolumeController.Instance.FadeMixerParam(
                VolumeController.Instance.masterMixer,
                "MusicVol",
                0f,
                3f
            );
        }

        //==================================================
        // START FIGHT E RIDAI CONTROLLI
        //==================================================
        if (bossManager != null)
            bossManager.IniziaCombattimento();

        if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic; 
        if (movement != null) movement.enabled = true;
        if (input != null) input.enabled = true;
    }

    public void ResetCamera()
    {
        alreadyStarted = false;

        SetTilemapsAlpha(muroRealTilemaps, 0f);
        SetTilemapsAlpha(muroFantasyTilemaps, 0f);

        if (muroReal != null) muroReal.SetActive(false);
        if (muroFantasy != null) muroFantasy.SetActive(false);

        if (bossCamera != null) bossCamera.Priority = 0;
        if (bossAudio != null) bossAudio.StopBossMusic();
        if (bossManager != null) bossManager.ResetInizio();
    }

    // CAMBIATO: Ora accetta un array di Tilemap
    private void SetTilemapsAlpha(Tilemap[] tilemaps, float alpha)
    {
        if (tilemaps == null) return;
        
        foreach (var tm in tilemaps)
        {
            if (tm == null) continue;
            Color c = tm.color;
            c.a = alpha;
            tm.color = c;
        }
    }

    // CAMBIATO: Ora accetta un array di Tilemap
    private IEnumerator FadeWall(Tilemap[] tilemaps)
    {
        if (tilemaps == null || tilemaps.Length == 0)
            yield break;

        SetTilemapsAlpha(tilemaps, 0f);

        float timer = 0f;

        while (timer < wallFadeDuration)
        {
            timer += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(0f, 1f, timer / wallFadeDuration);
            SetTilemapsAlpha(tilemaps, currentAlpha);
            yield return null;
        }

        SetTilemapsAlpha(tilemaps, 1f);
    }
}