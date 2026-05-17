using UnityEngine;

public class BossCameraTrigger : MonoBehaviour
{
    //==================================================
    // 🎥 IMPOSTAZIONI INQUADRATURA
    //==================================================
    [Header("Impostazioni Inquadratura")]
    public Transform puntoCentrale;

    [Tooltip("Quanto deve indietreggiare la camera?")]
    public float distanzaZ = -25f;

    public float velocitaTransizione = 3f;

    //==================================================
    // 🧱 BLOCCO ARENA (DUE MURI)
    //==================================================
    [Header("Blocco Arena")]
    // Abbiamo diviso il vecchio 'muroDietro' in due slot separati per l'arena
    public GameObject muroSinistro;
    public GameObject muroDestro;

    //==================================================
    // 🧠 STATO INTERNO E COMPONENTI
    //==================================================
    private Camera cam;
    private Behaviour cinemachineBrain;

    private bool isBossFightActive = false;

    //==================================================
    // INIZIALIZZAZIONE
    //==================================================
    private void Start()
    {
        cam = Camera.main;

        if (cam != null)
        {
            cinemachineBrain = cam.GetComponent("CinemachineBrain") as Behaviour;
        }

        // Assicura che ENTRAMBI i muri siano spenti all'inizio del livello
        if (muroSinistro != null)
            muroSinistro.SetActive(false);

        if (muroDestro != null)
            muroDestro.SetActive(false);
    }

    //==================================================
    // RILEVAMENTO ENTRATA GIOCATORE
    //==================================================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Controlla se l'oggetto entrato ha il tag "Player" e se la boss fight non è già attiva
        if (collision.CompareTag("Player") && !isBossFightActive)
        {
            isBossFightActive = true;

            // ATTIVAZIONE MURI: Chiudiamo il giocatore dentro l'arena
            if (muroSinistro != null)
                muroSinistro.SetActive(true);

            if (muroDestro != null)
                muroDestro.SetActive(true);

            // Spegne temporaneamente Cinemachine per dare il controllo del movimento a questo script
            if (cinemachineBrain != null)
                cinemachineBrain.enabled = false;
        }
    }

    //==================================================
    // MOVIMENTO CAMERA (LATE UPDATE)
    //==================================================
    private void LateUpdate()
    {
        // Se la boss fight è attiva, sposta la telecamera verso il centro dell'arena
        if (isBossFightActive && cam != null)
        {
            Vector3 targetPos = new Vector3(
                puntoCentrale.position.x,
                puntoCentrale.position.y,
                distanzaZ
            );

            // Spostamento fluido usando la funzione Lerp
            cam.transform.position = Vector3.Lerp(
                cam.transform.position,
                targetPos,
                Time.deltaTime * velocitaTransizione
            );
        }
    }

    //==================================================
    // RESET SISTEMA (CHIAMATA IN CASO DI MORTE/RESET)
    //==================================================
    public void ResetCamera()
    {
        isBossFightActive = false;

        // Spegne ENTRAMBI i muri quando il giocatore muore o la partita resetta
        if (muroSinistro != null)
            muroSinistro.SetActive(false);

        if (muroDestro != null)
            muroDestro.SetActive(false);

        // Riattiva Cinemachine per far seguire nuovamente il giocatore
        if (cinemachineBrain != null)
            cinemachineBrain.enabled = true;
    }
}