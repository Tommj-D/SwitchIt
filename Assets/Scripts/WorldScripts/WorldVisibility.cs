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
        bool shouldBeVisible =
            (isFantasyObject && WorldSwitch.isFantasyWorldActive) ||
            (!isFantasyObject && !WorldSwitch.isFantasyWorldActive);

        sr.enabled = shouldBeVisible;
        if (col != null)
            col.enabled = shouldBeVisible;
    }
}
