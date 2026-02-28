using System.Collections;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class SwitchLevel : MonoBehaviour
{
    public string nextSceneName;

    [Header("Teleport Settings")]
    public float absorbDuration = 1.2f;

    [Header("Effects")]
    public GameObject teleportEffect;
    public Transform magicStonePoint;
    public CinemachineCamera vcam;

    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            activated = true;
            StartCoroutine(LevelCompleteSequence(other.gameObject));
        }
    }

    private IEnumerator LevelCompleteSequence(GameObject player)
    {
        GameManager.Instance.isChangingLevel = true;

        // Blocca input e movimento
        PlayerInput input = player.GetComponent<PlayerInput>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (input != null) input.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        //BLOCCA CAMERA SU PIETRA
        if (vcam != null)
        {
            vcam.Follow = magicStonePoint;
        }

        // FX magico
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, player.transform.position, Quaternion.identity);
        }

        // Transizione audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.switchLevelSound);
            AudioManager.Instance.SetAudioState(AudioManager.AudioState.Transition);
        }

        if (VolumeController.Instance != null)
        {
            VolumeController.Instance.DuckMixer(VolumeController.Instance.masterMixer, "MusicTransitionVol", 6f, 0.3f);
            VolumeController.Instance.FadeMixerParam(VolumeController.Instance.masterMixer, "MusicTransitionLowPass", 1500f, 0.6f);
            VolumeController.Instance.FadeMixerParam(VolumeController.Instance.masterMixer, "MusicTransitionHightPass", 1500f, 0.6f);
            VolumeController.Instance.FadeMixerParam(VolumeController.Instance.masterMixer, "MusicTransitionPitch", 0.95f, 0.6f);
            VolumeController.Instance.FadeMixerParam(VolumeController.Instance.masterMixer, "SFXTransitionLowpass", 18000f, 0.6f);
}
        // Piccolo delay
        yield return new WaitForSeconds(0.1f);

        ShockWaveManager.Instance.CallShockWave(magicStonePoint.position, -0.5f);

        // Assorbimento verso la pietra magica
        yield return StartCoroutine(AbsorbPlayer(player));

        if (GameManager.Instance != null)
        {
            GameManager.Instance.isTestMode = false;
        }

        SceneController.Instance.LoadScene(nextSceneName);
    }

    private IEnumerator AbsorbPlayer(GameObject player)
    {
        SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();

        Vector3 startPos = player.transform.position;
        Vector3 targetPos = magicStonePoint.position;
        Vector3 startScale = player.transform.localScale;

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime / absorbDuration;

            player.transform.position = Vector3.Lerp(startPos, targetPos, t);
            player.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            if (sr)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1, 0, t);
                sr.color = c;
            }

            yield return null;
        }
    }
}
