using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFade : MonoBehaviour
{
    private Image fadeImage;

    private void Awake() 
    {
        fadeImage = GetComponent<Image>();
    }


    public IEnumerator FadeInCoroutine(float duration)
    {
        float targetAlpha = 1f - Mathf.Clamp01(LightManager.Instance.CurrentIntensity);

        Color startColor = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1);
        Color targetColor = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, targetAlpha);

        yield return FadeCoroutine(startColor, targetColor, duration);

        gameObject.SetActive(false);
    }


    public IEnumerator FadeOutCoroutine(float duration)
    {
        float currentAlpha = fadeImage.color.a;

        Color startColor = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, currentAlpha);
        Color targetColor = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1);
        gameObject.SetActive(true);

        yield return FadeCoroutine(startColor, targetColor, duration);
       
    }


    private IEnumerator FadeCoroutine(Color startColor, Color targetColor, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = targetColor; 
    }

    public void SetAlphaFromLight(float lightIntensity)
    {
        float alpha = 1f - Mathf.Clamp01(lightIntensity);

        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }

}

