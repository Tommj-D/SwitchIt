using UnityEngine;

public class WorldVisibility : MonoBehaviour
{
    public bool isFantasyObject = true;

    private SpriteRenderer sr;
    private Collider2D col;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void Start()
    {
        UpdateVisibility();
    }

    void Update()
    {
        UpdateVisibility();
    }

    void UpdateVisibility()
    {
        if (WorldSwitch.Instance == null) return;
       
        bool shouldBeVisible =
            (isFantasyObject && WorldSwitch.Instance.isFantasyWorldActive) ||
            (!isFantasyObject && !WorldSwitch.Instance.isFantasyWorldActive);

        sr.enabled = shouldBeVisible;
        if (col != null)
            col.enabled = shouldBeVisible;
    }
}
