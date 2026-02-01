using UnityEngine;
using UnityEngine.Rendering.Universal;

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

        // Smooth per il flip della posizione
        currentMaskOffset = Vector3.Lerp(
            currentMaskOffset,
            desiredOffset,
            maskFlipSpeed * Time.deltaTime
        );

        // Smooth della scala
        spriteMask.transform.localScale = Vector3.Lerp(
            spriteMask.transform.localScale,
            targetMaskScale,
            maskScaleSpeed * Time.deltaTime
        );

        targetMaskPosition = originalMaskPosition + currentMaskOffset;

        spriteMask.transform.localPosition = Vector3.Lerp(
            spriteMask.transform.localPosition,
            targetMaskPosition,
            maskPositionSpeed * Time.deltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (AudioManager.instance != null&&!isActivated)
        {
            AudioManager.instance.PlaySFX(AudioManager.instance.secretEntranceSound);
            isActivated = true;
        }

        isPlayerInsideCave = true;
        LightManager.Instance.EnterCave();

        if (spriteMask != null)
        {
            targetMaskScale = spriteMaskDimension;
            targetMaskPosition = originalMaskPosition + spriteMaskPosition;
        }
    }   


    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInsideCave = false;
        LightManager.Instance.ExitCave();

        if (spriteMask != null)
        {
            // Torna alla scala originale
            targetMaskScale = originalMaskScale;
            targetMaskPosition = originalMaskPosition;
        }
    }

}
