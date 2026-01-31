using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.InputSystem; // Fondamentale per risolvere l'errore!

public class VolumeController : MonoBehaviour
{
    public AudioMixer masterMixer;
    public GameObject volumeMenuCanvas;
    public Slider musicSlider;
    public Slider sfxSlider;

    private bool isMenuOpen = false;

    void Update()
    {
        // Nuovo modo di leggere il tasto ESC con l'Input System
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isMenuOpen)
                CloseMenu();
            else
                OpenMenu();
        }
    }

    public void OpenMenu()
{
    volumeMenuCanvas.SetActive(true);
    Time.timeScale = 0f; 
    isMenuOpen = true;
    
    // Queste righe servono a liberare il mouse dal gioco
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
    
    // Forza l'EventSystem a guardare il menu
    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
}

    public void CloseMenu()
    {
        volumeMenuCanvas.SetActive(false);
        Time.timeScale = 1f; 
        isMenuOpen = false;

        // Se il tuo gioco non è in prima persona, puoi lasciare il mouse visibile
        // Cursor.visible = false; 
    }

    public void SetMusicVolume(float volume)
    {
        // Imposta il volume nel mixer (Parametro: MusicVol)
        masterMixer.SetFloat("MusicVol", volume);
    }

    public void SetSFXVolume(float volume)
    {
        // Imposta il volume nel mixer (Parametro: SFXVol)
        masterMixer.SetFloat("SFXVol", volume);
    }
}