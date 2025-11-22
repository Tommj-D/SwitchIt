using UnityEngine;

public class WorldVisibility : MonoBehaviour
{
    [SerializeField] private bool isFantasyObject = true;

    private SpriteRenderer sr;
    private Collider2D col;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        // Iscriviti all'evento
        WorldSwitch.OnWorldChanged += HandleWorldChanged;
    }

    private void OnDisable()
    {
        // Annulla iscrizione per evitare memory leak / reference dangling
        WorldSwitch.OnWorldChanged -= HandleWorldChanged;
    }

    // Questo metodo viene chiamato ogni volta che il mondo cambia
    private void HandleWorldChanged(bool isFantasyActive)
    {
        bool shouldBeVisible = false;

        if (isFantasyObject)
        {
            if (isFantasyActive)
                shouldBeVisible = true;
            else
                shouldBeVisible = false;
        }
        else
        {
            if (!isFantasyActive)
                shouldBeVisible = true;
            else
                shouldBeVisible = false;
        }

        if (sr != null)
            sr.enabled = shouldBeVisible;

        if (col != null)
            col.enabled = shouldBeVisible;
    }
}
