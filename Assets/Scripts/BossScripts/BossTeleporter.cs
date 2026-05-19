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

        if (SceneController.Instance != null)
        {
            yield return StartCoroutine(
                SceneController.Instance.FadeOut(
                    SceneController.Instance.fadeDuration
                )
            );
        }

        //==================================================
        // TELETRASPORTO
        //==================================================

        player.transform.position = puntoDiArrivo.position;

        //==================================================
        // RESET LUCI CAVERNA
        //==================================================

        if (LightManager.Instance != null)
        {
            LightManager.Instance.ExitCave();
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
        // FADE IN
        //==================================================

        if (SceneController.Instance != null)
        {
            yield return StartCoroutine(
                SceneController.Instance.FadeIn(
                    SceneController.Instance.fadeDuration
                )
            );
        }

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
}