using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; 

    [Header("Sorgenti Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clip Audio")]
    public AudioClip backgroundMusic;
    [Header("Item")]
    public AudioClip coinSound;
    
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
    public AudioClip endLevelSound;
    public AudioClip switchLevelSound;
    public AudioClip secretEntranceSound;

    void Awake()
    {
        // Si assicura che ci sia solo un AudioManager
        if (instance == null) instance = this;
        else Destroy(gameObject);
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