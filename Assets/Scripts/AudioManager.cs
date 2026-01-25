using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; // Permette di accedervi da altri script

    [Header("Sorgenti Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clip Audio")]
    [Header("Clip Audio")]
    public AudioClip backgroundMusic;
    public AudioClip coinSound;
    public AudioClip checkpointSound;
    public AudioClip enemyDeathSound; // NUOVO: Trascina qui il suono del nemico sconfitto
    public AudioClip playerDeathSound; // NUOVO: Trascina qui il suono della morte del player

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