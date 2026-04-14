using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;

public class LevelSelectionManager : MonoBehaviour
{
    [Header("Bottoni dei Livelli")]
    // Trascina qui i bottoni in ordine: Livello 1 nell'elemento 0, Livello 2 nell'elemento 1, ecc.
    public UnityEngine.UI.Button[] levelButtons; 

    private int maxLevelUnlocked = 1;

    void Start()
    {
        // Appena si apre la scena, chiediamo a PlayFab a che punto è il giocatore
        GetPlayerProgression();
    }

    private void GetPlayerProgression()
    {
        // Crea la richiesta per leggere i dati del giocatore
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataReceived, OnError);
    }

    private void OnDataReceived(GetUserDataResult result)
    {
        // Controlliamo se nel "taccuino" c'è già la parola chiave "MaxLevelUnlocked"
        if (result.Data != null && result.Data.ContainsKey("MaxLevelUnlocked"))
        {
            // Se c'è, leggiamo il numero (convertendolo da testo a intero)
            maxLevelUnlocked = int.Parse(result.Data["MaxLevelUnlocked"].Value);
            Debug.Log("Dati trovati! Il giocatore è arrivato al livello: " + maxLevelUnlocked);
        }
        else
        {
            // Se non c'è, è un giocatore nuovo. Parte dal livello 1.
            maxLevelUnlocked = 1;
            Debug.Log("Nessun dato trovato. Nuovo giocatore, livello 1.");
        }

        // Ora che sappiamo il livello, aggiorniamo la grafica
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Scorriamo tutti i bottoni
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1; // L'elemento 0 dell'array corrisponde al Livello 1

            // Se il numero del livello è minore o uguale al massimo livello sbloccato...
            if (levelNumber <= maxLevelUnlocked)
            {
                levelButtons[i].interactable = true; // ...attiva il bottone!
            }
            else
            {
                levelButtons[i].interactable = false; // ...altrimenti lascialo spento.
            }
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("Errore PlayFab: " + error.GenerateErrorReport());
        // Se manca internet o c'è un errore, per sicurezza sblocchiamo solo l'1
        UpdateUI(); 
    }

    // Questa è la funzione che assegnerai ai tuoi bottoni nell'evento OnClick()
    public void LoadLevel(string nomeScena)
    {
        // Usiamo il TUO manager delle scene per fare un caricamento pulito!
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(nomeScena);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nomeScena);
        }
    } 
}