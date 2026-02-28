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
    public AudioMixerGroup musicFantasyGroup;
    public AudioMixerGroup sfxFantasyGroup;
    public enum AudioState
    {
        Normal,
        Transition,
        Fantasy
    }
    private AudioState currentState;

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

    void Start()
    {
        currentState = (AudioState)(-1);
        
        // Fa partire la musica di sottofondo all'inizio
        musicSource.clip = backgroundMusic;
        musicSource.Play();

        SetAudioState(AudioState.Normal);
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (VolumeController.Instance != null)
            VolumeController.Instance.RefreshUISliders();

        // Torna allo stato normale quando carichi una scena
        SetAudioState(AudioState.Normal);
        VolumeController.Instance.ResetGameplayVolumes();
        VolumeController.Instance.ResetAllTransitionParams(3f);
    }

    public void PlaySFX(AudioClip clip)
    {
        // Riproduce l'effetto sonoro senza interrompere quello precedente
        if (clip == null || sfxSource == null) return;

        sfxSource.PlayOneShot(clip);
    }

    public void SetAudioState(AudioState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        switch (currentState)
        {
            case AudioState.Normal:
                ApplyMixer(musicDefaultGroup, sfxDefaultGroup);
                break;

            case AudioState.Fantasy:
                ApplyMixer(musicFantasyGroup, sfxFantasyGroup);
                break;

            case AudioState.Transition:
                ApplyMixer(musicTransitionGroup, sfxTransitionGroup);
                break;
        }
    }

    private void ApplyMixer(AudioMixerGroup musicGroup, AudioMixerGroup sfxGroup)
    {
        if (musicSource != null && musicGroup != null)
            musicSource.outputAudioMixerGroup = musicGroup;

        if (sfxSource != null && sfxGroup != null)
            sfxSource.outputAudioMixerGroup = sfxGroup;
    }
}