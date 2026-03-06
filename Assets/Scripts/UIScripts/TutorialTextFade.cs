using System.Collections;
using UnityEngine;
using TMPro; // FONDAMENTALE: dice a Unity che stiamo usando TextMeshPro

public class TutorialTextFade : MonoBehaviour
{
    [Header("Impostazioni Testo")]
    public TextMeshPro testoTutorial; // Inseriremo la scritta qui
    public float durataFade = 1f;     // Quanto ci mette ad apparire (in secondi)

    private Coroutine fadeCoroutine;

    private void Start()
    {
        // All'inizio del gioco, rendiamo il testo subito invisibile (Alpha = 0)
        if (testoTutorial != null)
        {
            Color c = testoTutorial.color;
            c.a = 0f;
            testoTutorial.color = c;
        }
    }

    // Quando il Player ENTRA nella zona verde (Box Collider)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(EseguiFade(1f)); // 1 = Completamente visibile
        }
    }

    // Quando il Player ESCE dalla zona verde
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(EseguiFade(0f)); // 0 = Torna invisibile
        }
    }

    // La "Magia" che sfuma il colore gradualmente nel tempo
    private IEnumerator EseguiFade(float targetAlpha)
    {
        if (testoTutorial == null) yield break;

        Color coloreAttuale = testoTutorial.color;
        float startAlpha = coloreAttuale.a;
        float timer = 0f;

        while (timer < durataFade)
        {
            timer += Time.deltaTime;
            float avanzamento = timer / durataFade;
            
            // Lerp mescola dolcemente il valore di partenza con quello di arrivo
            coloreAttuale.a = Mathf.Lerp(startAlpha, targetAlpha, avanzamento);
            testoTutorial.color = coloreAttuale;
            
            yield return null;
        }

        // Assicuriamoci che alla fine sia esattamente al valore desiderato
        coloreAttuale.a = targetAlpha;
        testoTutorial.color = coloreAttuale;
    }
}