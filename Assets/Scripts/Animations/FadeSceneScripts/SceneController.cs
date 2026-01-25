using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    public float fadeDuration = 1f;

    private ScreenFade sceneFade;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sceneFade = GetComponentInChildren<ScreenFade>();
    }


    private IEnumerator Start()
    {
        sceneFade.gameObject.SetActive(true);
        yield return sceneFade.FadeInCoroutine(fadeDuration);
    }


    public void LoadScene(string sceneName)
    {
        LightManager.Instance.LockLight(
        LightManager.Instance.CurrentIntensity
    );

        StartCoroutine(LoadSceneCoroutine(sceneName));
    }
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        float startLight = LightManager.Instance.CurrentIntensity;

        // Fade out
        yield return FadeOutWithLight(fadeDuration);

        // Load scena
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return null;

        startLight = 0f;
        LightManager.Instance.ForceIntensity(startLight);
        sceneFade.SetAlphaFromLight(startLight);

        float targetLight = LightManager.Instance.outsideIntensity;
        // Fade in
        yield return FadeInWithLight(fadeDuration, targetLight);
    }

    private IEnumerator FadeOutWithLight(float duration)
    {
        float startLight = LightManager.Instance.CurrentIntensity;
        float startAlpha = sceneFade.GetComponent<UnityEngine.UI.Image>().color.a;

        float t = 0f;

        sceneFade.SetAlphaFromLight(startLight); 

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = Mathf.SmoothStep(0f, 1f, t);

            float lightValue = Mathf.Lerp(startLight, 0f, eased);
            float alphaValue = Mathf.Lerp(startAlpha, 1f, eased);

            LightManager.Instance.ForceIntensity(lightValue);
            sceneFade.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, alphaValue);

            yield return null;
        }
    }


    private IEnumerator FadeInWithLight(float duration, float targetLight)
    {
        float t = 0f;

        sceneFade.gameObject.SetActive(true);

        LightManager.Instance.ForceIntensity(0f);
        sceneFade.SetAlphaFromLight(0f);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = Mathf.SmoothStep(0f, 1f, t);

            float lightValue = Mathf.Lerp(0f, targetLight, eased);
            LightManager.Instance.ForceIntensity(lightValue);
            sceneFade.SetAlphaFromLight(lightValue);

            yield return null;
        }
        LightManager.Instance.UnlockLight();
    }
}
