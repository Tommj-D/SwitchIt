using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.EventSystems.EventTrigger;

public class CaveController : MonoBehaviour
{
    public SpriteMask spriteMask;
    private bool isPlayerInsideCave = false;
    private bool isActivated = false;

    [Header("Sprite Mask Settings")]
    public Vector3 spriteMaskDimension;
    public Vector3 spriteMaskPosition;
    private Vector3 currentMaskOffset;

    [Header("Scale Settings")]
    public float maskScaleSpeed = 5f;

    [Header("Position Settings")]
    public float maskPositionSpeed = 5f;
    public float maskFlipSpeed = 5f;

    public bool makeSoundEffect = true;
    public bool isCave = true;

    [Header("Exit Settings")]
    public bool resetAudioOnExit = true;
    public bool resetLightOnExit = true;

    private Vector3 originalMaskScale;
    private Vector3 targetMaskScale;

    private Vector3 originalMaskPosition;
    private Vector3 targetMaskPosition;

    private void Start()
    {
        if (spriteMask != null)
        {
            originalMaskScale = spriteMask.transform.localScale;
            originalMaskPosition = spriteMask.transform.localPosition;
        }

        targetMaskScale = originalMaskScale;
        targetMaskPosition = originalMaskPosition;

        currentMaskOffset = spriteMaskPosition;
    }


    private void Update()
    {
        if (spriteMask == null || !isPlayerInsideCave)
            return;

        // Calcola offset in base alla direzione del player
        Vector3 desiredOffset = PlayerMovement.isFacingRight
            ? spriteMaskPosition
            : new Vector3(-spriteMaskPosition.x, spriteMaskPosition.y, spriteMaskPosition.z);

        // Flip indipendente
        currentMaskOffset = Vector3.MoveTowards(
            currentMaskOffset,
            desiredOffset,
            maskFlipSpeed * Time.deltaTime
        );

        // Scala
        spriteMask.transform.localScale = Vector3.Lerp(
            spriteMask.transform.localScale,
            targetMaskScale,
            maskScaleSpeed * Time.deltaTime
        );

        // Posizione
        // Offset target (dipende dal player)
        Vector3 targetOffset = currentMaskOffset;

        // Posizione target finale
        Vector3 finalTargetPosition = originalMaskPosition + targetOffset;

        // Movimento verso la posizione finale
        spriteMask.transform.localPosition = Vector3.Lerp(
            spriteMask.transform.localPosition,
            finalTargetPosition,
            maskPositionSpeed * Time.deltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Questo avviene sempre
        isPlayerInsideCave = true;

        if (spriteMask != null)
        {
            targetMaskScale = spriteMaskDimension;
            targetMaskPosition = originalMaskPosition + spriteMaskPosition;
        }

        // Questo avvine solo se � una grotta
        if (isCave)
        {
            VolumeController.Instance.FadeMixerParam(VolumeController.Instance.masterMixer, "MusicLowpass", 800f, 0.4f);
            VolumeController.Instance.DuckMixer(VolumeController.Instance.masterMixer, "MusicVol", 3f, 0.4f);

            if (AudioManager.Instance != null && !isActivated && makeSoundEffect)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.secretEntranceSound);
                isActivated = true;
            }

            LightManager.Instance.EnterCave();
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Questo avviene sempre
        isPlayerInsideCave = false;

        if (spriteMask != null)
        {
            targetMaskScale = originalMaskScale;
            targetMaskPosition = originalMaskPosition;

            currentMaskOffset = spriteMaskPosition;

            spriteMask.transform.localScale = originalMaskScale;
            spriteMask.transform.localPosition = originalMaskPosition;
        }

        // Questo avvine solo se � una grotta
        if (isCave)
        {
            if (resetAudioOnExit)
            {
                if (GameManager.Instance != null &&
                    !GameManager.Instance.isChangingLevel)
                {
                    VolumeController.Instance.ResetGameplayVolumes(0.4f);
                }
            }

            if (resetLightOnExit)
            {
                LightManager.Instance.ExitCave();
            }
        }
    }
}
