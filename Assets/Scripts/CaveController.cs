using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CaveController : MonoBehaviour
{
    public SpriteMask spriteMask;

    [Header("Sprite Mask Settings")]
    //public bool modifySpriteMask = false;
    public float spriteMaskDimension = 1.5f;
    public float maskLerpSpeed = 5f;

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
        // Aggiorna la posizione target in base alla direzione del player
        Vector3 desiredOffset = PlayerMovement.isFacingRight
            ? maskOffset
            : new Vector3(-maskOffset.x, maskOffset.y, maskOffset.z);

        // Smooth transition dell’offset
        currentMaskOffset = Vector3.Lerp(
            currentMaskOffset,
            desiredOffset,
            maskFlipSpeed * Time.deltaTime
        );

        // Target position finale
        targetMaskPosition = originalMaskPosition + currentMaskOffset;

        // Aggiorna la posizione e scala effettiva della sprite mask
        spriteMask.transform.localPosition = Vector3.Lerp(
            spriteMask.transform.localPosition,
            targetMaskPosition,
            maskLerpSpeed * Time.deltaTime
        );

        spriteMask.transform.localScale = Vector3.Lerp(
            spriteMask.transform.localScale,
            targetMaskScale,
            maskLerpSpeed * Time.deltaTime
        );
    }
}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CaveLightManager.Instance.EnterCave();

        if (/*modifySpriteMask && */spriteMask != null)
        {
            targetMaskScale = originalMaskScale * spriteMaskDimension;
            targetMaskPosition = originalMaskPosition + maskOffset;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CaveLightManager.Instance.ExitCave();

        if (/*modifySpriteMask && */spriteMask != null)
        {
            targetMaskScale = originalMaskScale;
            targetMaskPosition = originalMaskPosition;
        }
    }
}
