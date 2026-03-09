using UnityEngine;

public class ButtonAnimationRelay : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponentInParent<Button>();
    }

    public void AnimationFinished()
    {
        button.AttivaOggetti();
    }
}