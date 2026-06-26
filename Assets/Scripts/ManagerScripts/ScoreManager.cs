using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // <-- AGGIUNTO: Necessario per ricaricare la scena

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("UI del Tempo")]
    public TextMeshProUGUI testoTempo; 

    [Header("Valori Punteggio")]
    public float tempoRimanente = 900f;
    public int puntiPerMoneta = 5;
    public int puntiPerNemico = 10;
    
    [Header("Progressione")]
    public int livelloSuccessivoDaSbloccare = 2;
    
    private int moneteRaccolte = 0;
    private int nemiciSconfitti = 0;
    private bool livelloFinito = false;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        // Se non siamo loggati (perché abbiamo avviato il livello direttamente per testarlo)
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.Log("Nessun login rilevato. Faccio un login automatico per il test...");
            
            var request = new LoginWithCustomIDRequest 
            { 
                CustomId = "SviluppatoreTest", 
                CreateAccount = true 
            };
            
            PlayFabClientAPI.LoginWithCustomID(request, 
                result => Debug.Log("Login di Test effettuato con successo!"), 
                error => Debug.LogError("Errore Login di Test: " + error.ErrorMessage));
        }
    }

    private void Update()
    {
        if (!livelloFinito && tempoRimanente > 0)
        {
            tempoRimanente -= Time.deltaTime;
            
            if (testoTempo != null)
            {
                testoTempo.text = Mathf.CeilToInt(tempoRimanente).ToString();
            }

            // --- NUOVA LOGICA: IL TEMPO È SCADUTO ---
            if (tempoRimanente <= 0)
            {
                tempoRimanente = 0; // Evita numeri negativi nell'UI
                TempoScaduto();
            }
        }
    }

    // --- NUOVA FUNZIONE: GESTIONE SCONFITTA PER TEMPO ---
    private void TempoScaduto()
    {
        Debug.Log("Tempo scaduto! Riavvio del livello corrente...");
        livelloFinito = true; // Blocca ulteriori calcoli

        // Ricarica la scena in cui ci troviamo in questo momento esatto.
        // I dati di PlayFab (livelli sbloccati online) rimarranno intatti, 
        // ma il livello fisico (posizione player, monete da raccogliere, ecc.) ripartirà da zero.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SegnalaMonetaRaccolta()
    {
        moneteRaccolte++;
    }

    public void SegnalaNemicoSconfitto()
    {
        nemiciSconfitti++;
    }

    public void CalcolaEInviaPunteggio()
    {
        if (livelloFinito) return;
        livelloFinito = true;

        int puntiTempo = Mathf.CeilToInt(tempoRimanente);

        int puntiMonete = moneteRaccolte * puntiPerMoneta;

        int puntiNemici = nemiciSconfitti * puntiPerNemico;

        int punteggioTotale = puntiTempo + puntiMonete + puntiNemici;

        Debug.Log("--- FINE LIVELLO ---");
        Debug.Log("Punti Tempo: " + puntiTempo);
        Debug.Log("Punti Monete: " + puntiMonete);
        Debug.Log("Punti Nemici: " + puntiNemici);
        Debug.Log("PUNTEGGIO FINALE TOTALE: " + punteggioTotale);

        InviaAPlayFab(punteggioTotale);
        SbloccaLivelloSuccessivo(livelloSuccessivoDaSbloccare);
    }

    private void InviaAPlayFab(int punteggio)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate { StatisticName = "ClassificaGlobale", Value = punteggio }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, 
            result => Debug.Log("Punteggio di " + punteggio + " salvato online!"), 
            error => Debug.LogError("Errore PlayFab: " + error.ErrorMessage));
    }

    private void SbloccaLivelloSuccessivo(int livelloDaSbloccare)
    {
        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            result =>
            {
                int livelloMassimo = 1;

                if (result.Data != null &&
                    result.Data.ContainsKey("MaxLevelUnlocked"))
                {
                    int.TryParse(result.Data["MaxLevelUnlocked"].Value, out livelloMassimo);
                }

                // Se il giocatore ha già sbloccato questo livello o uno superiore,
                // non facciamo nulla.
                if (livelloMassimo >= livelloDaSbloccare)
                {
                    Debug.Log("Livello già sbloccato, nessun aggiornamento necessario.");
                    return;
                }

                var request = new UpdateUserDataRequest
                {
                    Data = new Dictionary<string, string>
                    {
                        { "MaxLevelUnlocked", livelloDaSbloccare.ToString() }
                    }
                };

                PlayFabClientAPI.UpdateUserData(
                    request,
                    r => Debug.Log("Progresso salvato! Livello " + livelloDaSbloccare + " sbloccato."),
                    error => Debug.LogError("Errore salvataggio progresso: " + error.ErrorMessage)
                );
            },
            error => Debug.LogError("Errore lettura progresso: " + error.ErrorMessage)
        );
    }
}