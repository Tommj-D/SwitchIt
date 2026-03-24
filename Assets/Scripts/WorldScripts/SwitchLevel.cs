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
    public GameObject BurstEffect;

    public Transform magicStonePoint;
    public CinemachineCamera vcam;
    [Header("ShockWave")]
    public float shockWaveDuration = 1.4f;
    [Range(-5f, 5f)]
    public float shockWaveStrenght= -0.5f;

    private bool activated = false;

    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            activated = true;
            if (ScoreManager.instance != null)
            {
                ScoreManager.instance.CalcolaEInviaPunteggio();
            }
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
        GameObject fx1 = null; //Teleport effect
        GameObject fx2 = null; //Burst effect
        if (teleportEffect != null)
        {
            fx1 = Instantiate(teleportEffect, player.transform.position, Quaternion.identity);
            fx2 = Instantiate(BurstEffect, magicStonePoint.position, Quaternion.identity);

            //Faccio diventare la particella figlio del player per farla muovere con lui
            fx1.transform.SetParent(player.transform);
            fx1.transform.localPosition = Vector3.zero;

            ParticlesToTarget p = fx1.GetComponent<ParticlesToTarget>();
            if (p != null)
            {
                p.Init(magicStonePoint, null, 0, 6f, 0.2f);
            }
        }

        if(animator!=null)
        {
            animator.SetTrigger("Press");
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
            VolumeController.Instance.FadeMixerParam(VolumeController.Instance.masterMixer, "MusicTransitionPitch", 0.8f, 0.6f);
            VolumeController.Instance.FadeMixerParam(VolumeController.Instance.masterMixer, "SFXTransitionLowpass", 18000f, 0.6f);
        }
        // Piccolo delay
        yield return new WaitForSeconds(0.1f);

        //Shader Onda d'urto
        ShockWaveManager.Instance.CallShockWave(magicStonePoint.position, shockWaveStrenght, shockWaveDuration);

        // Assorbimento verso la pietra magica
        yield return StartCoroutine(AbsorbPlayer(player));

        //Elimino la particella figlio del player
        if (fx1 != null)
        {
            fx1.transform.SetParent(null); 
        }

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
