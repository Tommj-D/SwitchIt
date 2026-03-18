using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("UI del Tempo")]
    public TextMeshProUGUI testoTempo; 

    [Header("Valori Punteggio")]
    public float tempoRimanente = 900f;
    public int puntiPerMoneta = 5;
    public int puntiPerNemico = 10;
    
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
        }
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
}