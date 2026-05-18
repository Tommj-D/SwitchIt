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
    public GameObject muroSinistro;
    public GameObject muroDestro;

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

        if (muroSinistro != null) muroSinistro.SetActive(false);
        if (muroDestro != null) muroDestro.SetActive(false);
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
            if (muroSinistro != null) muroSinistro.SetActive(true);
            if (muroDestro != null) muroDestro.SetActive(true);

            // Spegne temporaneamente Cinemachine
            if (cinemachineBrain != null) cinemachineBrain.enabled = false;

            // 🎬 INIZIA LA SEQUENZA CINEMATOGRAFICA!
            StartCoroutine(SequenzaInizioBoss());
        }
    }

    //==================================================
    // SEQUENZA INTRO (NUOVO)
    //==================================================
    private IEnumerator SequenzaInizioBoss()
    {
        // 1. Ferma la musica esplorativa
        if (musicaLivello != null) musicaLivello.Stop();

        // 2. Fai ruggire il boss e ottieni la durata del suono
        float tempoAttesa = 1f; 
        if (bossManager != null)
        {
            tempoAttesa = bossManager.EmettiRuggito();
        }

        // 3. Pausa in silenzio mentre il mostro urla
        yield return new WaitForSeconds(tempoAttesa);

        // 4. Fai partire la colonna sonora del boss
        if (musicaBossFight != null) musicaBossFight.Play();

        // 5. Scatena il boss!
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

        if (muroSinistro != null) muroSinistro.SetActive(false);
        if (muroDestro != null) muroDestro.SetActive(false);

        if (cinemachineBrain != null) cinemachineBrain.enabled = true;

        // Ripristino Audio
        if (musicaBossFight != null) musicaBossFight.Stop();
        if (musicaLivello != null) musicaLivello.Play();

        // Rimette il boss al suo posto
        if (bossManager != null) bossManager.ResetInizio();
    }
}