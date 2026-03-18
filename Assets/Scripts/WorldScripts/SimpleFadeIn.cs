using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SimpleFadeIn : MonoBehaviour
{
    public float fadeTime = 1f;

    private SpriteRenderer sr;
    private Tilemap tm;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        tm = GetComponent<Tilemap>();
    }

    public void Appear()
{
    // Reset alpha a 0 PRIMA del fade
    if (sr != null)
    {
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;
    }

    if (tm != null)
    {
        Color c = tm.color;
        c.a = 0f;
        tm.color = c;
    }

    StartCoroutine(FadeRoutine());
}

    IEnumerator FadeRoutine()
    {
        yield return new WaitForEndOfFrame(); // aspetta che l'oggetto sia attivo

        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / fadeTime);

            if (sr != null)
            {
                Color c = sr.color;
                c.a = a;
                sr.color = c;
            }

            if (tm != null)
            {
                Color c = tm.color;
                c.a = a;
                tm.color = c;
            }

            yield return null;
        }
    }

    public float GetFadeTime()
    {
        return fadeTime;
    }

    public void SetAlphaInstant(float a)
    {
        if (sr != null)
        {
            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }

        if (tm != null)
        {
            Color c = tm.color;
            c.a = a;
            tm.color = c;
        }
    }
}