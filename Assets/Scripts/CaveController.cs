using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CaveController : MonoBehaviour
{
    public SpriteMask spriteMask;

    [Header("Sprite Mask Settings")]
    public bool modifySpriteMask = false;
    public float spriteMaskDimension = 1.5f;
    public float maskLerpSpeed = 5f;

    private Vector3 originalMaskScale;
    private Vector3 targetMaskScale;

    private void Start()
    {
        if (spriteMask != null)
            originalMaskScale = spriteMask.transform.localScale;

        targetMaskScale = originalMaskScale;
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
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CaveLightManager.Instance.EnterCave();

        if (modifySpriteMask && spriteMask != null)
        {
            targetMaskScale = originalMaskScale * spriteMaskDimension;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CaveLightManager.Instance.ExitCave();

        if (modifySpriteMask && spriteMask != null)
        {
            targetMaskScale = originalMaskScale;
        }
    }
}
