using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro; 
using UnityEngine.SceneManagement; 

public class PlayFabManager : MonoBehaviour
{
    [Header("Interfaccia Utente")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI testoMessaggio; // Il testo per i feedback a schermo

    private void Start()
    {
        // Svuota il messaggio all'avvio
        if (testoMessaggio != null)
        {
            testoMessaggio.text = "";
        }
    }

    // ==========================================
    // FUNZIONE DI REGISTRAZIONE
    // ==========================================
    public void RegisterButton()
    {
        if (passwordInput.text.Length < 6)
        {
            testoMessaggio.text = "La password deve essere di almeno 6 caratteri!";
            return;
        }

        testoMessaggio.text = "Caricamento...";

        var request = new RegisterPlayFabUserRequest
        {
            Email = emailInput.text,
            Password = passwordInput.text,
            RequireBothUsernameAndEmail = false // Usiamo solo l'email per semplificare
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        testoMessaggio.text = "Registrazione completata! Ora puoi fare il login.";
        Debug.Log("Account creato con successo su PlayFab!");
    }

    // ==========================================
    // FUNZIONE DI LOGIN
    // ==========================================
    public void LoginButton()
    {
        testoMessaggio.text = "Caricamento...";

        var request = new LoginWithEmailAddressRequest
        {
            Email = emailInput.text,
            Password = passwordInput.text
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        testoMessaggio.text = "Login effettuato con successo!";
        Debug.Log("Giocatore loggato! ID: " + result.PlayFabId);
        
        SceneController.Instance.LoadScene("Level_1");
    }

       private void OnError(PlayFabError error)
    {
        testoMessaggio.text = "Errore: " + error.ErrorMessage;
        Debug.LogError(error.GenerateErrorReport());
    }
}