using UnityEngine;
using System.Collections;

public class WorldSwitchTransition : MonoBehaviour
{
    [Header("Wave")]
    public GameObject fantasyWavePrefab;
    public GameObject realWavePrefab;
    public Transform player;
    public float waveDuration = 0.3f;
    public float waveMaxScale = 20f;

    [Header("Glitch")]
    public float glitchDuration = 0.1f;
    public float freezeTime = 0.05f;

    public IEnumerator PlayTransition(WorldSwitch worldSwitch)
    {
        bool goingToFantasy = !worldSwitch.isFantasyWorldActive;
        yield return StartCoroutine(FreezeEffect());
        yield return StartCoroutine(GlitchEffect());
        yield return StartCoroutine(WaveEffect(worldSwitch, goingToFantasy));
    }

    private IEnumerator FreezeEffect()
    {
        Time.timeScale = 0.2f;
        yield return new WaitForSecondsRealtime(freezeTime);
        Time.timeScale = 1f;
    }

    private IEnumerator GlitchEffect()
    {
        SpriteRenderer originalSprite = player.GetComponent<SpriteRenderer>();

        if (originalSprite == null)
            yield break;

        // Crea GameObject solo grafico
        GameObject ghost = new GameObject("PlayerGhost");

        SpriteRenderer ghostSprite = ghost.AddComponent<SpriteRenderer>();
        ghostSprite.sprite = originalSprite.sprite;
        ghostSprite.flipX = originalSprite.flipX;
        ghostSprite.sortingLayerID = originalSprite.sortingLayerID;
        ghostSprite.sortingOrder = originalSprite.sortingOrder - 1;

        ghostSprite.color = new Color(0.8f, 0f, 1f, 0.6f); 

        ghost.transform.position = player.position + new Vector3(0.15f, 0f, 0f);
        ghost.transform.localScale = player.localScale;

        yield return new WaitForSeconds(glitchDuration);

        Destroy(ghost);
    }


    private IEnumerator WaveEffect(WorldSwitch worldSwitch, bool goingToFantasy)
    {
        GameObject selectedPrefab = goingToFantasy ? fantasyWavePrefab : realWavePrefab;
        GameObject wave = Instantiate(selectedPrefab, player.position, Quaternion.identity);

        wave.transform.localScale = Vector3.zero;

        SpriteRenderer sr = wave.GetComponent<SpriteRenderer>();

        float baseAlpha = sr != null ? sr.color.a : 1f;

        Color startColor = goingToFantasy
        ? new Color(0.8f, 0f, 1f, 1f)        // viola
        : new Color(0.6f, 0.9f, 1f, 1f);     // azzurro chiaro

        // Colore finale: LUCE PURA
        Color pureLightColor = new Color(0.95f, 0.98f, 1f, 1f);



        float elapsed = 0f;
        bool worldChanged = false;

        while (elapsed < waveDuration)
        {
            float t = elapsed / waveDuration;

            // Easing più morbido
            float eased = Mathf.Sin(t * Mathf.PI * 0.5f);

            // Scala principale
            float scale = Mathf.Lerp(0f, waveMaxScale, eased);
            wave.transform.localScale = Vector3.one * scale;

            // Rotazione continua
            wave.transform.Rotate(0f, 0f, 400f * Time.deltaTime);



            if (sr != null)
            {
                float fade = Mathf.InverseLerp(0.5f, 1f, t);

                // Interpolazione verso luce pura
                Color currentColor = Color.Lerp(startColor, pureLightColor, fade);

                float finalAlpha = Mathf.Lerp(baseAlpha, 0f, fade);

                sr.color = new Color(currentColor.r, currentColor.g, currentColor.b, finalAlpha);
            }



            // Fade solo dopo metà
            /*if (sr != null && t > 0.5f)
            {
                float fade = Mathf.InverseLerp(0.5f, 1f, t);
                sr.color = new Color(0.8f, 0f, 1f, 1f - fade);
            }*/

            // Cambio mondo al picco
            if (!worldChanged && t >= 0.45f)
            {
                worldSwitch.ApplyWorldChange();
                worldChanged = true;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Implosione finale
        Vector3 startScale = wave.transform.localScale;

        float overshoot = 1.1f;
        wave.transform.localScale = startScale * overshoot;

        Destroy(wave);
    }

}
