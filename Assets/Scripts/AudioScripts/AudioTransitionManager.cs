using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioTransitionManager : MonoBehaviour
{
    [Header("Gruppi Transizione")]
    public AudioMixerGroup musicTransitionGroup;
    public AudioMixerGroup sfxTransitionGroup;

    private Dictionary<AudioSource, AudioMixerGroup> originalGroups = new Dictionary<AudioSource, AudioMixerGroup>();

    /// <summary>
    /// Sposta tutti i suoni esistenti sul TransitionMixer
    /// </summary>
    public void EnterTransition()
    {
        AudioSource[] allSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (var src in allSources)
        {
            if (!originalGroups.ContainsKey(src))
                originalGroups[src] = src.outputAudioMixerGroup;

            if (src.outputAudioMixerGroup.name.ToLower().Contains("music"))
                src.outputAudioMixerGroup = musicTransitionGroup;
            else
                src.outputAudioMixerGroup = sfxTransitionGroup;
        }
    }

    /// <summary>
    /// Riporta tutti i suoni ai gruppi originali
    /// </summary>
    public void ExitTransition()
    {
        foreach (var kvp in originalGroups)
        {
            if (kvp.Key != null)
                kvp.Key.outputAudioMixerGroup = kvp.Value;
        }
        originalGroups.Clear();
    }
}
