using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioTransitionManager : MonoBehaviour
{
    [System.Serializable]
    public class MixerTransition
    {
        public AudioMixerGroup originalGroup;
        public AudioMixerGroup transitionGroup;
    }

    [Header("Mappature Transizione")]
    public List<MixerTransition> mixerTransitions = new List<MixerTransition>();

    private Dictionary<AudioSource, AudioMixerGroup> originalGroups = new Dictionary<AudioSource, AudioMixerGroup>();

    public void EnterTransition()
    {
        AudioSource[] allSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (var src in allSources)
        {
            if (!originalGroups.ContainsKey(src))
                originalGroups[src] = src.outputAudioMixerGroup;

            foreach (var mapping in mixerTransitions)
            {
                if (src.outputAudioMixerGroup == mapping.originalGroup)
                {
                    src.outputAudioMixerGroup = mapping.transitionGroup;
                    break;
                }
            }
        }
    }

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