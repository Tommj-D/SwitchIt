using UnityEngine;
using System.Collections; // Necessario per le Coroutine

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
    public GameObject muroReal;
    public GameObject muroFantasy;

    //==================================================
    // 🎬 REGIA E MUSICA (NUOVO)
    //==================================================
    [Header("Regia e Musica")]
    public BossManager bossManager;
    
    [Tooltip("L'AudioSource che suona la musica del livello (da spegnere)")]
    public AudioSource musicaLivello;
    
    [Tooltip("L'AudioSource che suona la musica del Boss (da accendere)")]
    public AudioSource musicaBossFight;

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

        if (muroReal != null) muroReal.SetActive(false);
        if (muroFantasy != null) muroFantasy.SetActive(false);
    }

    //==================================================
    // RILEVAMENTO ENTRATA GIOCATORE
    //==================================================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isBossFightActive)
        {
            isBossFightActive = true;

            // ATTIVAZIONE MURI: Chiudiamo il giocatore dentro l'arena
            if (muroReal != null) muroReal.SetActive(true);
            if (muroFantasy != null) muroFantasy.SetActive(true);

            // Spegne temporaneamente Cinemachine
            if (cinemachineBrain != null) cinemachineBrain.enabled = false;

            // 🎬 INIZIA LA SEQUENZA CINEMATOGRAFICA!
            StartCoroutine(SequenzaInizioBoss());
        }
    }

    //==================================================
    // SEQUENZA INTRO (PRECISIONE ASSOLUTA)
    //==================================================
    private IEnumerator SequenzaInizioBoss()
    {
        // 1. Calcoliamo la posizione finale dove deve arrivare la telecamera
        Vector3 destinazioneCamera = new Vector3(
            puntoCentrale.position.x,
            puntoCentrale.position.y,
            distanzaZ
        );

        // 2. Mettiamo in pausa questo script finché la telecamera non è arrivata!
        // HO CAMBIATO LA TOLLERANZA: da 1f (1 metro) a 0.1f (10 centimetri).
        // Ora aspetta che la telecamera abbia completato visibilmente tutta la frenata.
        while (cam != null && Vector3.Distance(cam.transform.position, destinazioneCamera) > 0.1f)
        {
            yield return null; 
        }

        // --- DA QUI IN POI LA TELECAMERA SI È FERMATA COMPLETAMENTE ---

        // 3. ATTIVAZIONE MURI: Sbatte le porte e chiude il giocatore nell'arena
        if (muroReal != null) muroReal.SetActive(true);
        if (muroFantasy != null) muroFantasy.SetActive(true);

        // 4. Ferma la musica esplorativa
        if (musicaLivello != null) musicaLivello.Stop();

        // 5. Fai ruggire il boss e ottieni la durata del suono
        float tempoAttesa = 1f; 
        if (bossManager != null)
        {
            tempoAttesa = bossManager.EmettiRuggito();
        }

        // 6. Pausa in silenzio mentre il mostro urla
        yield return new WaitForSeconds(tempoAttesa);

        // 7. Fai partire la colonna sonora del boss
        if (musicaBossFight != null) musicaBossFight.Play();

        // 8. Scatena il boss!
        if (bossManager != null) bossManager.IniziaCombattimento();
    }

    //==================================================
    // MOVIMENTO CAMERA (LATE UPDATE)
    //==================================================
    private void LateUpdate()
    {
        if (isBossFightActive && cam != null)
        {
            Vector3 targetPos = new Vector3(
                puntoCentrale.position.x,
                puntoCentrale.position.y,
                distanzaZ
            );

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

        if (muroReal != null) muroReal.SetActive(false);
        if (muroFantasy != null) muroFantasy.SetActive(false);

        if (cinemachineBrain != null) cinemachineBrain.enabled = true;

        // Ripristino Audio
        if (musicaBossFight != null) musicaBossFight.Stop();
        if (musicaLivello != null) musicaLivello.Play();

        // Rimette il boss al suo posto
        if (bossManager != null) bossManager.ResetInizio();
    }
}