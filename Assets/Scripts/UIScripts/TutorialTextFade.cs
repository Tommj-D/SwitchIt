using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class TutorialTextFade : MonoBehaviour
{
    [Header("Impostazioni Testo")]
    public TextMeshPro testoTutorial;
    public float durataFade = 1f;
    public Key tastoScomparsa; 
    public static bool tutorialCompletato = false; 

    [Header("Floating Effect")]
    public  float floatAmplitude = 0.2f;
    public float floatAmplitudeX = 0.1f;
    public  float floatSpeed = 2f;

    private Vector3 startPos;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (tutorialCompletato)
        {
            gameObject.SetActive(false);
            return;
        }

        startPos = transform.position;

        if (testoTutorial != null)
        {
            Color c = testoTutorial.color;
            c.a = 0f;
            testoTutorial.color = c;
        }
    }

    private void Update()
    {
        if (tutorialCompletato) return;

        // Movimento fluttuante
        float offsetY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        float offsetX = Mathf.Cos(Time.time * floatSpeed * 0.8f) * floatAmplitudeX;

        transform.position = startPos + new Vector3(offsetX, offsetY, 0f);

        if (Keyboard.current != null && Keyboard.current[tastoScomparsa].wasPressedThisFrame)
        {
            tutorialCompletato = true;

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(EseguiFade(0f));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (tutorialCompletato) return;

        if (collision.CompareTag("Player"))
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(EseguiFade(1f));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (tutorialCompletato) return;

        if (collision.CompareTag("Player") && gameObject.activeInHierarchy)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(EseguiFade(0f));
        }
    }

    private IEnumerator EseguiFade(float targetAlpha)
    {
        if (testoTutorial == null) yield break;

        Color coloreAttuale = testoTutorial.color;
        float startAlpha = coloreAttuale.a;
        float timer = 0f;

        while (timer < durataFade)
        {
            timer += Time.deltaTime;
            coloreAttuale.a = Mathf.Lerp(startAlpha, targetAlpha, timer / durataFade);
            testoTutorial.color = coloreAttuale;
            yield return null;
        }

        coloreAttuale.a = targetAlpha;
        testoTutorial.color = coloreAttuale;

        if (targetAlpha == 0f)
        {
            gameObject.SetActive(false);
        }
    }
}