using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CaveController : MonoBehaviour
{
    public SpriteMask spriteMask;
    private bool isPlayerInsideCave = false;

    [Header("Sprite Mask Settings")]
    public Vector3 spriteMaskDimension;
    public Vector3 spriteMaskPosition;
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

        targetMaskPosition = originalMaskPosition + currentMaskOffset;
        spriteMask.transform.localPosition = targetMaskPosition;


    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInsideCave = true;
        CaveLightManager.Instance.EnterCave();

        if (spriteMask != null)
        {
            targetMaskScale = spriteMaskDimension;
            targetMaskPosition = originalMaskPosition + spriteMaskPosition;

            spriteMask.transform.localScale = targetMaskScale;
        }
    }   


    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInsideCave = false;
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
