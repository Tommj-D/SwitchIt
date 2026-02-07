using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class VolumeController : MonoBehaviour
{
    public static VolumeController Instance;

    [Header("Audio Mixer")]
    public AudioMixer masterMixer;

    [Header("UI Menu")]
    public GameObject volumeMenuCanvas; // UI scene-based
    public Slider musicSlider;          // UI scene-based
    public Slider sfxSlider;            // UI scene-based

    private bool isMenuOpen = false;

    private float defaultMusicVolume = 0f;
    private Coroutine musicFadeRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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

    //----------GESTIONE UI, MENU, SLIDER----------//
    public void SetUI(GameObject menuCanvas, Slider music, Slider sfx)
    {
        volumeMenuCanvas = menuCanvas;
        musicSlider = music;
        sfxSlider = sfx;

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            musicSlider.value = defaultMusicVolume;
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            sfxSlider.value = 1f;
        }
    }
    public void OpenMenu()
    {
            if (volumeMenuCanvas == null)
                return;

            volumeMenuCanvas.SetActive(true);
            Time.timeScale = 0f;
            isMenuOpen = true;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseMenu()
    {
        if (volumeMenuCanvas == null)
            return;

        volumeMenuCanvas.SetActive(false);
        Time.timeScale = 1f;
        isMenuOpen = false;
    }

    // SET VOLUME
    public void SetMusicVolume(float volume)
    {
        defaultMusicVolume = volume;
        if (masterMixer != null)
            masterMixer.SetFloat("MusicVol", volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (masterMixer != null)
            masterMixer.SetFloat("SFXVol", volume);
    }

    //----------DUCKING MUSICA----------//
    public void DuckMusic(float targetVolume, float duration)
    {
        if (this == null) return; // sicurezza
        if (musicFadeRoutine != null) StopCoroutine(musicFadeRoutine);

        musicFadeRoutine = StartCoroutine(FadeMusic(targetVolume, duration));
    }

    public void RestoreMusic(float duration)
    {
        DuckMusic(defaultMusicVolume, duration);
    }

    private IEnumerator FadeMusic(float targetVolume, float duration)
    {
        if (masterMixer == null) yield break;

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
        if (masterMixer == null) return;
        StartCoroutine(LowpassFadeRoutine(target, duration));
    }

    private IEnumerator LowpassFadeRoutine(float target, float duration)
    {
        if (masterMixer == null) yield break;

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

    public void FadeMasterLowpass(float target, float duration)
    {
        if (masterMixer == null) return;
        StartCoroutine(LowpassMasterFadeRoutine(target, duration));
    }

    private IEnumerator LowpassMasterFadeRoutine(float target, float duration)
    {
        if (masterMixer == null) yield break;

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


    // Aggiorna i riferimenti agli slider UI ogni volta che cambi scena
    public void RefreshUISliders()
    {
        // Se la UI non si è ancora registrata, esco
        if (volumeMenuCanvas == null || musicSlider == null || sfxSlider == null)
            return;

        musicSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        musicSlider.value = defaultMusicVolume;

        sfxSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        sfxSlider.value = 1f;
    }

    public void ClearUI(GameObject uiRoot)
    {
        if (volumeMenuCanvas == uiRoot)
        {
            volumeMenuCanvas = null;
            musicSlider = null;
            sfxSlider = null;
            isMenuOpen = false;
        }
    }
}