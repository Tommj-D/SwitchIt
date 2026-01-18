using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CaveController : MonoBehaviour
{
    public SpriteMask spriteMask;

    [Header("Sprite Mask Settings")]
    public bool modifySpriteMask = false;
    public float spriteMaskDimension = 1.5f;
    public float maskLerpSpeed = 5f;

    [Header("Sprite Mask Offset")]
    public Vector3 maskOffset; 

    private Vector3 originalMaskScale;
    private Vector3 targetMaskScale;

    private Vector3 originalMaskPosition;
    private Vector3 targetMaskPosition;

    private int facingDirection = 1; // 1 = right, -1 = left

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
        if (modifySpriteMask && spriteMask != null)
        {
            spriteMask.transform.localScale = Vector3.Lerp(
                spriteMask.transform.localScale,
                targetMaskScale,
                maskLerpSpeed * Time.deltaTime
            );

            spriteMask.transform.localPosition = Vector3.Lerp(
                spriteMask.transform.localPosition,
                targetMaskPosition,
                maskLerpSpeed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CaveLightManager.Instance.EnterCave();

        if (modifySpriteMask && spriteMask != null)
        {
            targetMaskScale = originalMaskScale * spriteMaskDimension;
            UpdateTargetPosition();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CaveLightManager.Instance.ExitCave();

        if (modifySpriteMask && spriteMask != null)
        {
            targetMaskScale = originalMaskScale;
            targetMaskPosition = originalMaskPosition;
        }
    }

    //CHIAMATO DAL PLAYER QUANDO CAMBIA DIREZIONE
    public void SetFacingDirection(int direction)
    {
        facingDirection = direction;
        UpdateTargetPosition();
    }

    private void UpdateTargetPosition()
    {
        Vector3 offset = maskOffset;

        offset.x *= facingDirection;

        targetMaskPosition = originalMaskPosition + offset;
    }
}
