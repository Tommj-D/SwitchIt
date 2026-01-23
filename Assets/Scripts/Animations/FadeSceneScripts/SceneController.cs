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
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }


    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        yield return sceneFade.FadeOutCoroutine(fadeDuration);
        yield return SceneManager.LoadSceneAsync(sceneName);
    }
}
