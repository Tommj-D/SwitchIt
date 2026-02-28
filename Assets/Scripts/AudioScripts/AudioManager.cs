using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; 

    [Header("Sorgenti Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clip Audio")]
    public AudioClip backgroundMusic;
    [Header("Item")]
    public AudioClip coinSound;
    public AudioClip chestSound;
    public AudioClip keySound;
    public AudioClip chestLockedSound;

    [Header("Enemies")]
    public AudioClip slimeDeathSound; 
    public AudioClip shakeSound;
    public AudioClip maceCrashSound;
    public AudioClip spikeCrashSound;
    
    [Header("Player")]
    public AudioClip jumpSound;
    public AudioClip jumpLanding;
    public AudioClip playerDeathSound;
    public AudioClip walkSound;

    [Header("World")]
    public AudioClip checkpointSound;
    public AudioClip respawnSound;
    public AudioClip switchLevelSound;
    public AudioClip secretEntranceSound;
    
    [Header("Mixer Groups")]
    public AudioMixerGroup musicDefaultGroup;
    public AudioMixerGroup musicTransitionGroup;
    public AudioMixerGroup sfxDefaultGroup;
    public AudioMixerGroup sfxTransitionGroup;
    public enum AudioState
    {
        Normal,
        Transition
    }
    private AudioState currentState = AudioState.Normal;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (VolumeController.Instance != null)
            VolumeController.Instance.RefreshUISliders();
            
        // Assicura che ogni volta che carichiamo una scena, torniamo al gruppo audio di default
        ExitTransitionState();
    }

    void Start()
    {
        // Fa partire la musica di sottofondo all'inizio
        musicSource.clip = backgroundMusic;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        // Riproduce l'effetto sonoro senza interrompere quello precedente
        sfxSource.PlayOneShot(clip);
    }

    // Entra nello stato di transizione (es. cambio livello)
    public void EnterTransitionState()
    {
        if (currentState == AudioState.Transition)
            return;

        currentState = AudioState.Transition;

        musicSource.outputAudioMixerGroup = musicTransitionGroup;
        sfxSource.outputAudioMixerGroup = sfxTransitionGroup;
    }

    //Ritorna al volume default
    public void ExitTransitionState()
    {
        if (currentState == AudioState.Normal)
            return;

        currentState = AudioState.Normal;

        musicSource.outputAudioMixerGroup = musicDefaultGroup;
        sfxSource.outputAudioMixerGroup = sfxDefaultGroup;

        if (VolumeController.Instance != null)
            VolumeController.Instance.ResetAllTransitionParams(0.3f);
    }
}