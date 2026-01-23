using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFade : MonoBehaviour
{
    private Image fadeImage;
    SceneController sceneController;

    private void Awake() 
    {
        fadeImage = GetComponent<Image>();
        sceneController = GetComponentInParent<SceneController>();
    }


    public IEnumerator FadeInCoroutine(float duration)
    {
        Color startColor = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1);
        Color targetColor = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0);

        yield return FadeCoroutine(startColor, targetColor, duration);

        gameObject.SetActive(false);
    }


    public IEnumerator FadeOutCoroutine(float duration)
    {
        Color startColor = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0);
        Color targetColor = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1);
        gameObject.SetActive(true);

        yield return FadeCoroutine(startColor, targetColor, duration);
       
    }


    private IEnumerator FadeCoroutine(Color startColor, Color targetColor, float duration)
    {
        float elapsedTime = 0;
        float elapsedPercentage = 0;

        while(elapsedPercentage < 1)
        {
            elapsedPercentage = elapsedTime / duration;
            fadeImage.color = Color.Lerp(startColor, targetColor, elapsedPercentage);

            yield return null;

            elapsedTime += Time.deltaTime;
        }
    }
}

