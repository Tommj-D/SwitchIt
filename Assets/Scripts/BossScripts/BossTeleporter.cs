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
    [SerializeField] private float tempoCamminataForzata = 0.4f;
    [SerializeField] private float velocitaUscita = 8f;
    [SerializeField] private float direzioneUscita = 1f;

    [Header("Light Fade")]
    [SerializeField] private float fadeLightDuration = 0.5f;

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
            FadeLight(0f, fadeLightDuration)
        );

        //==================================================
        // TELETRASPORTO
        //==================================================

        player.transform.position = puntoDiArrivo.position;

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
        // FADE IN
        //==================================================

        yield return StartCoroutine(
            FadeLight(1f, fadeLightDuration)
        );

        //==================================================
        // CAMMINATA AUTOMATICA
        //==================================================

        float timer = 0f;

        while (timer < tempoCamminataForzata)
        {
            timer += Time.deltaTime;

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(
                    velocitaUscita * direzioneUscita,
                    rb.linearVelocity.y
                );
            }

            yield return null;
        }

        //==================================================
        // RIDÀ CONTROLLI
        //==================================================

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