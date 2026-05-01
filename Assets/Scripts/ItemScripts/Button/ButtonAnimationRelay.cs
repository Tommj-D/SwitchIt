using UnityEngine;

public class ButtonAnimationRelay : MonoBehaviour
{
    private SlimeSpawnerButton parentButton;

    private void Awake()
    {
        parentButton = GetComponentInParent<SlimeSpawnerButton>();
    }

    // Questa viene chiamata dall'Animation Event
    public void OnResetAnimationFinished()
    {
        if (parentButton != null)
        {
            parentButton.OnResetAnimationFinished();
        }
    }
}