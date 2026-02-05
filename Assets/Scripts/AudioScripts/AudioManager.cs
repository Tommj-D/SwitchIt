using UnityEngine;

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
    

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
}