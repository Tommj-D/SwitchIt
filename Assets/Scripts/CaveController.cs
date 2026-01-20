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
    }


    private void Update()
    {
        if (spriteMask != null)
        {
            // Aggiorna la posizione target in base alla direzione del player
            Vector3 flippedOffset = PlayerMovement.isFacingRight
                ? maskOffset
                : new Vector3(-maskOffset.x, maskOffset.y, maskOffset.z);

            targetMaskPosition = originalMaskPosition + flippedOffset;

            // Lerp verso la posizione e scala target
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
