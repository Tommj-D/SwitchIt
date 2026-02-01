using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class VolumeController : MonoBehaviour
{
    public static VolumeController Instance;

    public AudioMixer masterMixer;
    public GameObject volumeMenuCanvas;
    public Slider musicSlider;
    public Slider sfxSlider;

    private bool isMenuOpen = false;

    private float defaultMusicVolume = 0f;
    private Coroutine musicFadeRoutine;

    void Awake()
    {
        Instance = this;
        masterMixer.GetFloat("MusicVol", out defaultMusicVolume);
        masterMixer.SetFloat("MasterLowpass", 22000f); // stato normale

    }

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
    }

    public void SetMusicVolume(float volume)
    {
        defaultMusicVolume = volume;
        masterMixer.SetFloat("MusicVol", volume);
    }

    public void SetSFXVolume(float volume)
    {
        // Imposta il volume nel mixer (Parametro: SFXVol)
        masterMixer.SetFloat("SFXVol", volume);
    }

    //----------DUCKING MUSICA----------//
    public void DuckMusic(float targetVolume, float duration)
    {
        if (musicFadeRoutine != null)
            StopCoroutine(musicFadeRoutine);

        musicFadeRoutine = StartCoroutine(FadeMusic(targetVolume, duration));
    }

    public void RestoreMusic(float duration)
    {
        DuckMusic(defaultMusicVolume, duration);
    }

    private IEnumerator FadeMusic(float targetVolume, float duration)
    {
        masterMixer.GetFloat("MusicVol", out float startVolume);

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float vol = Mathf.Lerp(startVolume, targetVolume, t / duration);
            masterMixer.SetFloat("MusicVol", vol);
            yield return null;
        }

        masterMixer.SetFloat("MusicVol", targetVolume);
    }

    //----------OFFUSCAMENTO MUSICA----------//
    public void FadeMusicLowpass(float target, float duration)
    {
        StartCoroutine(LowpassFadeRoutine(target, duration));
    }

    private IEnumerator LowpassFadeRoutine(float target, float duration)
    {
        masterMixer.GetFloat("MusicLowpass", out float start);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float value = Mathf.Lerp(start, target, t / duration);
            masterMixer.SetFloat("MusicLowpass", value);
            yield return null;
        }

        masterMixer.SetFloat("MusicLowpass", target);
    }

    //----------OFFUSCAMENTO MASTER (Musica + SFX)----------//
    public void FadeMasterLowpass(float target, float duration)
    {
        StartCoroutine(LowpassMasterFadeRoutine(target, duration));
    }

    private IEnumerator LowpassMasterFadeRoutine(float target, float duration)
    {
        masterMixer.GetFloat("MasterLowpass", out float start);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float value = Mathf.Lerp(start, target, t / duration);
            masterMixer.SetFloat("MasterLowpass", value);
            yield return null;
        }

        masterMixer.SetFloat("MasterLowpass", target);
    }
}