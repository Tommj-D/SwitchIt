using UnityEngine;
using System.Collections;

public class WorldSwitchTransition : MonoBehaviour
{
    [Header("Wave")]
    public GameObject wavePrefab;
    public Transform player;
    public float waveDuration = 0.3f;
    public float waveMaxScale = 20f;

    [Header("Glitch")]
    public float glitchDuration = 0.1f;
    public float freezeTime = 0.05f;

    public IEnumerator PlayTransition(WorldSwitch worldSwitch)
    {
        yield return StartCoroutine(FreezeEffect());
        yield return StartCoroutine(GlitchEffect());
        yield return StartCoroutine(WaveEffect(worldSwitch));
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

        ghostSprite.color = new Color(0.8f, 0f, 1f, 0.6f); // Viola glow

        ghost.transform.position = player.position + new Vector3(0.15f, 0f, 0f);
        ghost.transform.localScale = player.localScale;

        yield return new WaitForSeconds(glitchDuration);

        Destroy(ghost);
    }


    private IEnumerator WaveEffect(WorldSwitch worldSwitch)
    {
        GameObject wave = Instantiate(wavePrefab, player.position, Quaternion.identity);
        Transform cam = Camera.main.transform;
        Vector3 camStartPos = cam.position;

        wave.transform.localScale = Vector3.zero;

        SpriteRenderer sr = wave.GetComponent<SpriteRenderer>();

        float elapsed = 0f;
        bool worldChanged = false;

        while (elapsed < waveDuration)
        {
            float shakeAmount = 0.05f;
            cam.position = camStartPos + Random.insideUnitSphere * shakeAmount;
            float t = elapsed / waveDuration;

            // Easing più morbido
            float eased = Mathf.Sin(t * Mathf.PI * 0.5f);

            // Scala principale
            float scale = Mathf.Lerp(0f, waveMaxScale, eased);
            wave.transform.localScale = Vector3.one * scale;

            // Rotazione continua
            wave.transform.Rotate(0f, 0f, 400f * Time.deltaTime);

            // Fade solo dopo metà
            if (sr != null && t > 0.5f)
            {
                float fade = Mathf.InverseLerp(0.5f, 1f, t);
                sr.color = new Color(0.8f, 0f, 1f, 1f - fade);
            }

            // Cambio mondo al picco
            if (!worldChanged && t >= 0.45f)
            {
                worldSwitch.ApplyWorldChange();
                worldChanged = true;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.position = camStartPos;

        // Implosione finale
        Vector3 startScale = wave.transform.localScale;

        float overshoot = 1.1f;
        wave.transform.localScale = startScale * overshoot;

        Destroy(wave);
    }

}
