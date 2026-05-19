using UnityEngine;
using UnityEngine.SceneManagement; // Indispensabile per gestire il cambio scena

public class MenuButtonController : MonoBehaviour
{
    [Header("Impostazioni Scena")]
    [Tooltip("Menu")]
    public string nomeScenaMenu = "MainMenu";

    // Questa è la funzione pubblica che il bottone attiverà al click
    public void TornaAlMenu()
    {
        SceneManager.LoadScene(nomeScenaMenu);
    }
}