using UnityEngine;

public class WorldVisibility : MonoBehaviour
{
    [SerializeField] private bool isFantasyObject = true;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Questo metodo viene chiamato ogni volta che il mondo cambia
    private void HandleWorldChanged(bool isFantasyActive)
    {
        if (isFantasyObject)
        {
            // Se è un oggetto fantasy, attivalo solo quando isFantasyActive==true
            gameObject.SetActive(isFantasyActive);
        }
        else
        {
            // Se non è un oggetto fantasy (quindi "reale"), attivalo quando isFantasyActive==false
            gameObject.SetActive(!isFantasyActive);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
