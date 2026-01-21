using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CaveController : MonoBehaviour
{
    public SpriteMask spriteMask;

    [Header("Sprite Mask Settings")]
    public Vector3 spriteMaskTargetScale;

    [Header("Sprite Mask Offset")]
    public Vector3 maskOffset;
    private Vector3 currentMaskOffset;

    [Header("Flip Settings")]
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

        currentMaskOffset = maskOffset;

    }


    private void Update()
    {
        if (spriteMask != null)
        {
            // Calcola offset in base alla direzione del player
            Vector3 desiredOffset = PlayerMovement.isFacingRight
                ? maskOffset
                : new Vector3(-maskOffset.x, maskOffset.y, maskOffset.z);

            // Smooth per il flip della posizione
            currentMaskOffset = Vector3.Lerp(
                currentMaskOffset,
                desiredOffset,
                maskFlipSpeed * Time.deltaTime
            );

            targetMaskPosition = originalMaskPosition + currentMaskOffset;
            spriteMask.transform.localPosition = targetMaskPosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CaveLightManager.Instance.EnterCave();

        if (spriteMask != null)
        {
            targetMaskScale = spriteMaskTargetScale;
            targetMaskPosition = originalMaskPosition + maskOffset;

            spriteMask.transform.localScale = targetMaskScale;
        }
    }   


    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CaveLightManager.Instance.ExitCave();

        if (spriteMask != null)
        {
            // Torna alla scala originale
            targetMaskScale = originalMaskScale;
            targetMaskPosition = originalMaskPosition;

            spriteMask.transform.localScale = targetMaskScale;
        }
    }

}
