using System.Collections;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.switchLevelSound);
        }

        // Transizione audio magica

        AudioTransitionManager transition = AudioManager.Instance.GetComponent<AudioTransitionManager>();

        // Inizio transizione
        transition.EnterTransition();

        yield return new WaitForSeconds(0.1f);

        VolumeController.Instance.DuckMusic(-3f, 0.5f);
        VolumeController.Instance.FadeMixerParam("MusicLowpass", 1200f, 0.5f);
        VolumeController.Instance.FadeMixerParam("MusicPitch", 0.55f, 0.5f);
        VolumeController.Instance.FadeMixerParam("SFXLowpass", 16000f, 0.5f);
        VolumeController.Instance.FadeMixerParam("SFXPitch", 0.9f, 0.5f);

        // Piccolo delay o animazione
        yield return new WaitForSeconds(0.1f);

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
