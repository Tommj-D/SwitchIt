using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFade : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 1f;

    public void FadeOutIn(System.Action onFadeComplete)
    {
        StartCoroutine(FadeRoutine(onFadeComplete));
    }

    private IEnumerator FadeRoutine(System.Action onFadeComplete)
    {
        // Fade to black
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            SetAlpha(alpha);
            yield return null;
        }

        // Callback: il giocatore può essere respawnato qui
        onFadeComplete?.Invoke();

        // Fade back to transparent
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            SetAlpha(alpha);
            yield return null;
        }
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = Mathf.Clamp01(alpha);
            fadeImage.color = c;
        }
    }
}

