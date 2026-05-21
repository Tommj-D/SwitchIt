using UnityEngine;

public class BossAudioController : MonoBehaviour
{
    [Header("Boss SFX")]
    [SerializeField] private AudioClip roarSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;

    [Header("Boss Music")]
    [SerializeField] private AudioClip bossMusic;

    //==================================================
    // SFX
    //==================================================

    public float PlayRoar()
    {
        PlaySFX(roarSound);

        if (roarSound != null)
            return roarSound.length;

        return 1.5f;
    }

    public void PlayHit()
    {
        PlaySFX(hitSound);
    }

    public void PlayDeath()
    {
        PlaySFX(deathSound);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlaySFX(clip);
    }

    //==================================================
    // MUSIC
    //==================================================

    public void StartBossMusic()
    {
        AudioManager.Instance.PlayMusic(bossMusic);
    }

    public void StopBossMusic()
    {
        AudioManager.Instance.StopMusic();
    }
}