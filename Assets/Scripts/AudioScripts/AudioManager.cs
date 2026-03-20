using System.Collections;
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
    public AudioClip buttonSound;

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
    public AudioClip hintSound;
    public AudioClip glowingSound;
    public AudioClip wallDisappearingSound;
    
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
    }

    public void PlaySFX(AudioClip clip)
    {
        // Riproduce l'effetto sonoro senza interrompere quello precedente
        if (clip == null || sfxSource == null) return;

        sfxSource.PlayOneShot(clip);
    }

    // Overload per riprodurre un effetto sonoro con un pitch specifico
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        // Crea un GameObject temporaneo
        GameObject tempGO = new GameObject("SFX_" + clip.name);
        AudioSource tempSource = tempGO.AddComponent<AudioSource>();

        tempSource.clip = clip;
        tempSource.volume = volume;
        tempSource.pitch = pitch;
        tempSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup; // Mantieni eventuali effetti globali
        tempSource.Play();

        // Distruggi l'oggetto dopo la durata del clip
        Destroy(tempGO, clip.length / pitch); // Considera pitch nella durata
    }

    private IEnumerator PlaySFXRoutine(AudioClip clip, float volume, float pitch)
    {
        float originalPitch = sfxSource.pitch;

        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, volume);

        yield return null;

        sfxSource.pitch = originalPitch;
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

    // Metodo per resettare l'audio quando il giocatore respawna
    public IEnumerator ResetAudioOnPlayerSpawn(float transitionTime = 3.5f)
    {
        if (VolumeController.Instance == null)
            yield break;

        // Trova un'istanza di PlayerRespawn nella scena
        PlayerRespawn playerRespawn = Object.FindFirstObjectByType<PlayerRespawn>();

        //per evitare che uno cambi dimensione prima che la musica torni allo stato normale
        WorldSwitch.Instance.canSwitchWorld = false; 

        //Se il giocatore sta morendo torno subito allo stato normale e solo la musica va resettata
        if (playerRespawn != null && playerRespawn.IsDying())
        {
            WorldSwitch.Instance.canSwitchWorld = true; 
            // Torna allo stato normale
            SetAudioState(AudioState.Normal);

            VolumeController.Instance.ResetGameplayVolumes(2f);
        }

        //Altrimenti se sta facendo altre transizioni esempio fine livello o switch mondo, aspetto un po' prima di tornare allo stato normale 
        //rettando prima i volumi e parametri di transizione
        else
        {
            VolumeController.Instance.ResetGameplayVolumes(transitionTime);
            VolumeController.Instance.ResetAllTransitionParams(transitionTime);

            //aspetta che fniscano le transitioni prima di tornare allo stato normale
            yield return new WaitForSecondsRealtime(transitionTime+1f);

            // Torna allo stato normale
            SetAudioState(AudioState.Normal);
            WorldSwitch.Instance.canSwitchWorld = true;
        }
    }
}