using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class SceneController : MonoBehaviour
{
    public float fadeDuration = 1f;

    private ScreenFade sceneFade;


    private void Awake()
    {
        sceneFade = GetComponentInChildren<ScreenFade>();
    }


    private IEnumerator Start()
    {
        yield return sceneFade.FadeInCoroutine(fadeDuration);
    }


    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }


    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        yield return sceneFade.FadeOutCoroutine(fadeDuration);
        yield return SceneManager.LoadSceneAsync(sceneName);
    }
}
