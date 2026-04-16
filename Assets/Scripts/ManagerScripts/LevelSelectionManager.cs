using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;
using TMPro; 

public class LevelSelectionManager : MonoBehaviour
{
    [Header("Bottoni dei Livelli")]
    public UnityEngine.UI.Button[] levelButtons; 

    private int maxLevelUnlocked = 1;

    void Start()
    {
        GetPlayerProgression();
    }

    private void GetPlayerProgression()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataReceived, OnError);
    }

    private void OnDataReceived(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("MaxLevelUnlocked"))
        {
            maxLevelUnlocked = int.Parse(result.Data["MaxLevelUnlocked"].Value);
        }
        else
        {
            maxLevelUnlocked = 1;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1; 
            
            Transform numeroFiglio = levelButtons[i].transform.Find("number");
            TextMeshProUGUI testoNumero = null;
            
            if (numeroFiglio != null)
            {
                testoNumero = numeroFiglio.GetComponent<TextMeshProUGUI>();
            }

            if (levelNumber <= maxLevelUnlocked)
            {
                levelButtons[i].interactable = true; // SBLOCCATO
                
                // Mettiamo il colore solido (Alpha = 1f)
                if (testoNumero != null)
                {
                    Color c = testoNumero.color;
                    c.a = 1f;
                    testoNumero.color = c;
                }
            }
            else
            {
                levelButtons[i].interactable = false; // BLOCCATO
                
                // Mettiamo il colore trasparente (Alpha = 0.4f)
                if (testoNumero != null)
                {
                    Color c = testoNumero.color;
                    c.a = 0.4f;
                    testoNumero.color = c;
                }
            }
        }
    }

    private void OnError(PlayFabError error)
    {
        UpdateUI(); 
    }

    public void LoadLevel(string nomeScena)
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(nomeScena);
        }
        else
        {
            SceneManager.LoadScene(nomeScena);
        }
    }
}