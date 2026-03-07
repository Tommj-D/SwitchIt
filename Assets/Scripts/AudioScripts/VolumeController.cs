using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{


    public static VolumeController Instance;


    // ==============================
    // NOMI PARAMETRI MIXER (costanti per evitare errori di scrittura)
    // ==============================
    private const string MUSICMASTER_VOL = "MusicMasterVol";
    private const string SFXMASTER_VOL = "SFXMasterVol";
    private const string SFX_VOL = "SFXVol";
    private const string MUSIC_VOL = "MusicVol";
    private const string MUSIC_LP = "MusicLowpass";

    private const string MUSIC_TRANSITION_VOL = "MusicTransitionVol";
    private const string MUSIC_TRANSITION_PITCH = "MusicTransitionPitch";
    private const string MUSIC_TRANSITION_LP = "MusicTransitionLowPass";
    private const string MUSIC_TRANSITION_HP = "MusicTransitionHightPass";
    private const string SFX_TRANSITION_LP = "SFXTransitionLowpass";

    private const string SFX_FANTASY_LP = "SFXFantasyLowpass";

    // Tiene traccia delle coroutine attive per ogni parametro
    // Serve per evitare che più fade scrivano contemporaneamente sullo stesso parametro
    private Dictionary<string, Coroutine> activeFades = new Dictionary<string, Coroutine>();

    // Salva i valori originali dei parametri del mixer
    // Così possiamo sempre tornare allo stato iniziale corretto
    private Dictionary<string, float> defaultParams = new Dictionary<string, float>();

    [Header("Audio Mixer")]
    public AudioMixer masterMixer;

    [Header("UI Menu")]
    public GameObject volumeMenuCanvas;
    public Slider musicSlider;
    public Slider sfxSlider;

    private bool isMenuOpen = false;

    // Salviamo il volume di default della musica e SFX
    // Questi valori vengono aggiornati quando l'utente cambia il volume tramite gli slider
    private float userMusicVolume = 0f; 
    private float userSFXVolume = 0f;
    // Questi valori vengono salvati all'avvio e rappresentano i livelli di volume "normali" durante il gameplay, prima di qualsiasi transizione o ducking
    private float baseMusicGameplayVolume = 0f; 
    private float baseSFXGameplayVolume = 0f;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Salva i volumi iniziali dal mixer
        masterMixer.GetFloat(MUSICMASTER_VOL, out userMusicVolume);
        masterMixer.GetFloat(SFXMASTER_VOL, out userSFXVolume);

        masterMixer.GetFloat(MUSIC_VOL, out baseMusicGameplayVolume);
        masterMixer.GetFloat(SFX_VOL, out baseSFXGameplayVolume);

        // Salva i valori di default dei parametri di transizione
        SaveDefaultParam(MUSIC_TRANSITION_VOL);
        SaveDefaultParam(MUSIC_TRANSITION_PITCH);
        SaveDefaultParam(MUSIC_TRANSITION_LP);
        SaveDefaultParam(MUSIC_TRANSITION_HP);
        SaveDefaultParam(SFX_TRANSITION_LP);
        SaveDefaultParam(SFX_FANTASY_LP);
    }

    // Salva il valore iniziale di un parametro del mixer
    // Serve per poterlo ripristinare correttamente dopo una transizione
    private void SaveDefaultParam(string param)
    {
        if (masterMixer.GetFloat(param, out float value))
            defaultParams[param] = value;
    }

    void Update()
    {
        // Gestione apertura/chiusura menu con ESC usando il nuovo Input System
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isMenuOpen)
                CloseMenu();
            else
                OpenMenu();
        }
    }

    // ==========================================================
    // GESTIONE UI
    // ==========================================================

    // Ricollega gli slider quando cambi scena
    // Utile perché la UI viene distrutta ma il VolumeController no (DontDestroyOnLoad)
    public void RefreshUISliders()
    {
        if (volumeMenuCanvas == null || musicSlider == null || sfxSlider == null)
            return;

        musicSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        musicSlider.value = userMusicVolume;

        sfxSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        sfxSlider.value = userSFXVolume;
    }

    // Pulisce i riferimenti UI quando una scena viene distrutta
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

    // Imposta nuovi riferimenti UI quando entri in una scena
    public void SetUI(GameObject menuCanvas, Slider music, Slider sfx)
    {
        volumeMenuCanvas = menuCanvas;
        musicSlider = music;
        sfxSlider = sfx;

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            musicSlider.value = userMusicVolume;
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            sfxSlider.value = userSFXVolume;
        }
    }

    // Apre il menu volume e mette il gioco in pausa
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

    // Chiude il menu volume e ripristina il tempo di gioco
    public void CloseMenu()
    {
        if (volumeMenuCanvas == null)
            return;

        volumeMenuCanvas.SetActive(false);
        Time.timeScale = 1f;
        isMenuOpen = false;
    }

    // ==========================================================
    // SET VOLUME BASE
    // ==========================================================

    // Imposta il volume musica nel mixer
    public void SetMusicVolume(float volume)
    {
        userMusicVolume = volume;

        if (masterMixer != null)
            masterMixer.SetFloat(MUSICMASTER_VOL, volume);
    }

    public void SetSFXVolume(float volume)
    {
        userSFXVolume = volume;

        if (masterMixer != null)
            masterMixer.SetFloat(SFXMASTER_VOL, volume);
    }

    // ==========================================================
    // DUCKING
    // ==========================================================

    // Riduce temporaneamente un parametro (es: volume)
    // amount = quanto abbassare
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
            float value = Mathf.Lerp(originalValue, targetValue, Mathf.SmoothStep(0f, 1f, t / fadeTime));
            mixer.SetFloat(paramName, value);
            yield return null;
        }

        mixer.SetFloat(paramName, targetValue);
    }

    // ==========================================================
    // FADE PARAMETRO GENERICO
    // ==========================================================

    // Effettua una transizione fluida verso un valore target
    // Blocca eventuali fade precedenti sullo stesso parametro
    public void FadeMixerParam(AudioMixer mixer, string paramName, float targetValue, float duration)
    {
        if (mixer == null) return;

        if (activeFades.ContainsKey(paramName))
        {
            StopCoroutine(activeFades[paramName]);
            activeFades.Remove(paramName);
        }

        Coroutine c = StartCoroutine(FadeMixerParamRoutine(mixer, paramName, targetValue, duration));
        activeFades.Add(paramName, c);
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

    // ==========================================================
    // RESET
    // ==========================================================

    // Ripristina musica ai valori base istantaneamente
    public void ResetGameplayVolumes()
    {
        if (masterMixer == null) return;

        masterMixer.SetFloat(MUSIC_VOL, baseMusicGameplayVolume);
        masterMixer.SetFloat(SFX_VOL, baseSFXGameplayVolume);
        masterMixer.SetFloat(MUSIC_TRANSITION_LP, 22000f);
    }

    // Ripristina musica con fade
    public void ResetGameplayVolumes(float fadeTime)
    {
        if (masterMixer == null) return;

        FadeMixerParam(masterMixer, MUSIC_VOL, baseMusicGameplayVolume, fadeTime);
        FadeMixerParam(masterMixer, SFX_VOL, baseSFXGameplayVolume, fadeTime);
        FadeMixerParam(masterMixer, MUSIC_LP, 22000f, fadeTime);
    }

    // Ripristina tutti i parametri di transizione
    // Usa i valori salvati all'avvio
    public void ResetAllTransitionParams(float fadeTime)
    {
        foreach (var param in defaultParams)
        {
            FadeMixerParam(masterMixer, param.Key, param.Value, fadeTime);
        }
    }

    public void resetMixerParam(AudioMixer mixer, string paramName, float duration, float delay = 0f)
    {
        if (mixer == null) return;

        if (activeFades.TryGetValue(paramName, out Coroutine routine))
        {
            if (routine != null)
                StopCoroutine(routine);

            activeFades.Remove(paramName);
        }

        Coroutine c = StartCoroutine(ResetMixerParamRoutine(mixer, paramName, duration, delay));
        activeFades.Add(paramName, c);
    }

    private IEnumerator ResetMixerParamRoutine(AudioMixer mixer, string paramName, float duration, float delay)
    {
        if (mixer == null) yield break;

        yield return new WaitForSecondsRealtime(delay);

        if (!defaultParams.ContainsKey(paramName))
            yield break;

        float targetValue = defaultParams[paramName];

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
}