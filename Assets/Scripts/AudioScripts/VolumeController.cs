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
    public AudioMixer transitionMixer;
    public AudioMixer fantasyWorldMixer;

    [Header("UI Menu")]
    public GameObject volumeMenuCanvas; 
    public Slider musicSlider;          
    public Slider sfxSlider;            

    private bool isMenuOpen = false;

    private float defaultMusicVolume = 0f;
    private float defaultSFXVolume = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        masterMixer.GetFloat("MusicVol", out defaultMusicVolume);
        masterMixer.GetFloat("SFXVol", out defaultSFXVolume);
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
    // Aggiorna i riferimenti agli slider UI ogni volta che cambi scena
    public void RefreshUISliders()
    {
        if (volumeMenuCanvas == null || musicSlider == null || sfxSlider == null)
            return;

        musicSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        musicSlider.value = defaultMusicVolume;

        sfxSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        sfxSlider.value = defaultSFXVolume;
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
            sfxSlider.value = defaultSFXVolume;
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
        defaultSFXVolume = volume;
        if (masterMixer != null)
            masterMixer.SetFloat("SFXVol", volume);
    }

    //----------DUCKING----------//
    public void DuckMixer(AudioMixer mixer, string paramName, float amount, float fadeTime)
    {
        StartCoroutine(DuckMixerRoutine(mixer, paramName, amount, fadeTime));
    }

    private IEnumerator DuckMixerRoutine(
     AudioMixer mixer,
     string paramName,
     float amount,
     float fadeTime)
    {
        if (mixer == null)
            yield break;

        if (!mixer.GetFloat(paramName, out float originalValue))
            yield break;

        float targetValue = originalValue - amount;

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float value = Mathf.Lerp(originalValue, targetValue, t / fadeTime);
            mixer.SetFloat(paramName, value);
            yield return null;
        }

        mixer.SetFloat(paramName, targetValue);
    }

    //------------------------------------//
    //Da usare solo se non so a quale parameto voglio tornare, altrimenti uso FadeMixerParam con il valore target specifico
    public void RestoreMixerParam(AudioMixer mixer, string paramName, float fadeTime)
    {
        StartCoroutine(RestoreMixerParamRoutine(mixer, paramName, fadeTime));
    }

    private IEnumerator RestoreMixerParamRoutine(
        AudioMixer mixer,
        string paramName,
        float fadeTime)
    {
        if (mixer == null)
            yield break;

        if (!mixer.GetFloat(paramName, out float currentValue))
            yield break;

        float targetValue = 0f;

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float value = Mathf.Lerp(currentValue, targetValue, t / fadeTime);
            mixer.SetFloat(paramName, value);
            yield return null;
        }

        mixer.SetFloat(paramName, targetValue);
    }
    //------------------------------------//

    //----------FADE PARAMETER----------//
    public void FadeMixerParam(AudioMixer mixer, string paramName, float targetValue, float duration)
    {
        StartCoroutine(FadeMixerParamRoutine(mixer, paramName, targetValue, duration));
    }

    private IEnumerator FadeMixerParamRoutine(AudioMixer mixer, string paramName, float targetValue, float duration)
    {
        if (mixer == null) yield break;

        mixer.GetFloat(paramName, out float startValue);

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float value = Mathf.Lerp(startValue, targetValue, t / duration);
            mixer.SetFloat(paramName, value);
            yield return null;
        }

        mixer.SetFloat(paramName, targetValue);
    }


    //----------RESET----------//
    public void ResetMusicState()
    {
        if (masterMixer==null || transitionMixer==null) return;

        masterMixer.SetFloat("MusicVol", defaultMusicVolume);
        masterMixer.SetFloat("MusicLowpass", 22000f);

        transitionMixer.SetFloat("MusicVol", defaultMusicVolume);
        transitionMixer.SetFloat("MusicLowpass", 22000f);

    }

    public void ResetMusicState(float fadeTime)
    {
        if (masterMixer == null || transitionMixer == null) return;

        FadeMixerParam(masterMixer, "MusicVol", defaultMusicVolume, fadeTime);
        FadeMixerParam(masterMixer, "MusicLowpass", 22000f, fadeTime);

        FadeMixerParam(transitionMixer, "MusicVol", defaultMusicVolume, fadeTime);
        FadeMixerParam(transitionMixer, "MusicLowpass", 22000f, fadeTime);
    }
}