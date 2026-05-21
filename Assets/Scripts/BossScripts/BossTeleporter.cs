using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class BossTeleporter : MonoBehaviour
{
    [Header("Destinazione")]
    [SerializeField] private Transform puntoDiArrivo;

    [Header("Cambio Confiner")]
    [SerializeField] private Collider2D nuovoConfiner;

    [Header("Effetto Uscita")]
    [SerializeField] private float velocitaUscita = 8f;

    [Header("Entrata Cinematica")]
    [SerializeField] private GameObject movementBlocker;
    [SerializeField] private float durataFadeOut = 1.2f;
    [SerializeField] private float durataFadeIn = 1.2f;

    [SerializeField] private float tempoSchermoNero = 0.3f;
    [SerializeField] private float distanzaMinimaPerControllo = 4f;

    [Header("Boss Room Effects")]
    [SerializeField] private BossRoomFogController bossRoomFogController;
    
    private bool isTeleporting = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isTeleporting)
        {
            StartCoroutine(TeleportSequence(collision.gameObject));
        }
    }

    private IEnumerator TeleportSequence(GameObject player)
    {
        isTeleporting = true;

        //==================================================
        // COMPONENTI PLAYER
        //==================================================

        PlayerInput input = player.GetComponent<PlayerInput>();
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        //==================================================
        // BLOCCA CONTROLLI
        //==================================================

        if (input != null)
            input.enabled = false;

        if (movement != null)
            movement.enabled = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        //==================================================
        // ENTRATA CAVERNA TRANSIZIONE AUDIO
        //==================================================

        if (VolumeController.Instance != null)
        {
            VolumeController.Instance.FadeMixerParam(
                VolumeController.Instance.masterMixer,
                "MusicVol",
                -40f,
                2f
            );

            VolumeController.Instance.FadeMixerParam(
                VolumeController.Instance.masterMixer,
                "MusicLowpass",
                800f,
                0.6f
            );
        }

        //==================================================
        // SUONO PASSI 
        //==================================================

        if (AudioManager.Instance.enteringCaveFootstepSound != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.enteringCaveFootstepSound);

        //==================================================
        // FADE OUT
        //==================================================

        yield return StartCoroutine(
            FadeLight(0f, durataFadeOut)
        );

        //==================================================
        // TELETRASPORTO
        //==================================================

        player.transform.position = puntoDiArrivo.position;

        player.transform.localScale = new Vector3(
            Mathf.Abs(player.transform.localScale.x),
            player.transform.localScale.y,
            player.transform.localScale.z
        );

        if (bossRoomFogController != null)
        {
            bossRoomFogController.UpdateBossRoomVisuals();
        }
        //==================================================
        // CAMBIO CONFINER
        //==================================================

        if (nuovoConfiner != null)
        {
            CinemachineCamera activeCam =
                FindFirstObjectByType<CinemachineCamera>();

            if (activeCam != null)
            {
                CinemachineConfiner2D currentConfiner =
                    activeCam.GetComponent<CinemachineConfiner2D>();

                if (currentConfiner != null)
                {
                    currentConfiner.BoundingShape2D = nuovoConfiner;

                    currentConfiner.InvalidateBoundingShapeCache();
                }
            }
        }

        //==================================================
        // SNAP CAMERA
        //==================================================

        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(
                puntoDiArrivo.position.x,
                puntoDiArrivo.position.y,
                mainCam.transform.position.z
            );
        }

        //==================================================
        // CAMMINATA AUTOMATICA + FADE IN
        //==================================================

        //prima un attimo di schermo nero per far avvenire tutte le azioni
        yield return new WaitForSeconds(tempoSchermoNero);
        
        Vector3 startPos = player.transform.position;

        // Avvia subito il movimento
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                velocitaUscita,
                rb.linearVelocity.y
            );
        }

        // Fade in mentre il player cammina
        yield return StartCoroutine(
            FadeLight(1f, durataFadeIn)
        );

        // Continua a camminare finché non arriva alla distanza richiesta
        while (
            player.transform.position.x <
            startPos.x + distanzaMinimaPerControllo
        )
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(
                    velocitaUscita,
                    rb.linearVelocity.y
                );
            }

            yield return null;
        }

        //==================================================
        // RIDÀ CONTROLLI
        //==================================================

        if (movementBlocker != null)
        {
            movementBlocker.SetActive(true);
        }

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );
        }

        if (movement != null)
            movement.enabled = true;

        if (input != null)
            input.enabled = true;

        isTeleporting = false;

    }

    /// <summary>
    /// Coroutine per sfumare l'intensità della luce. Può essere usata per creare un effetto di transizione più fluida quando il player entra o esce dalla caverna, o in qualsiasi altro momento in cui si desidera modificare l'illuminazione in modo graduale.
    /// </summary>
    /// <param name="targetIntensity"></param>
    /// <param name="duration"></param>
    /// <returns></returns>//
    private IEnumerator FadeLight(float targetIntensity, float duration)
    {
        if (LightManager.Instance == null)
            yield break;

        float startIntensity = LightManager.Instance.globalLight.intensity;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;

            float intensity = Mathf.Lerp(
                startIntensity,
                targetIntensity,
                t
            );

            LightManager.Instance.ForceIntensity(intensity);

            yield return null;
        }

        LightManager.Instance.ForceIntensity(targetIntensity);
    }
}