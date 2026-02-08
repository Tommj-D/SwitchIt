using UnityEngine;
using UnityEngine.Audio;

public class AudioSnapshotController : MonoBehaviour
{
    [Header("Audio Mixer Snapshots")]
    public AudioMixerSnapshot gameplaySnapshot;
    public AudioMixerSnapshot transitionSnapshot;

    [Header("Transition Settings")]
    public float transitionTime = 0.4f;

    public void EnterTransition()
    {
        if (transitionSnapshot != null)
            transitionSnapshot.TransitionTo(transitionTime);
    }

    public void ExitTransition()
    {
        if (gameplaySnapshot != null)
            gameplaySnapshot.TransitionTo(transitionTime);
    }
}
