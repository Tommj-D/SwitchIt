using UnityEngine;

[RequireComponent(typeof(SpriteMask))]
[RequireComponent(typeof(SpriteRenderer))]
public class MaskFollower : MonoBehaviour
{
    [Header("Target")]
    public Transform player;                // assegna il transform del player

    [Header("Movement")]
    public Vector3 offset = new Vector3(0f, 0f, 0f);
    [Range(0f, 50f)] public float followSpeed = 20f; // smoothing (use 0 for snap)

    [Header("Mask size (world units)")]
    public float radius = 1.0f;             // raggio desiderato in unità world
    public bool updateScaleEveryFrame = false; // se false scala una sola volta in Start()

    // component cache
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            Debug.LogWarning("MaskFollower: manca SpriteRenderer sullo stesso GameObject.");
    }

    void Start()
    {
        if (player == null)
            Debug.LogWarning("MaskFollower: assegna il player nel inspector.");

        // calcola e applica la scala iniziale
        ApplyScaleForRadius();
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 target = player.position + offset;
        if (followSpeed <= 0f)
            transform.position = target;
        else
            transform.position = Vector3.Lerp(transform.position, target, Mathf.Clamp01(Time.deltaTime * followSpeed));

        if (updateScaleEveryFrame)
            ApplyScaleForRadius();
    }

    void ApplyScaleForRadius()
    {
        if (sr == null || sr.sprite == null) return;

        // dimensione in world units del diametro dello sprite (scale = 1)
        float spriteWorldDiameter = sr.sprite.bounds.size.x;
        if (spriteWorldDiameter <= 0.0001f) return;

        float desiredDiameter = radius * 2f;
        float requiredScale = desiredDiameter / spriteWorldDiameter;

        // applica scala uniforme su X e Y
        transform.localScale = new Vector3(requiredScale, requiredScale, 1f);
    }
}

