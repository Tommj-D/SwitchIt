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
    [Header("WaveColor")]
    public Color fantasyWaveColor = new Color(0.8f, 0f, 1f, 1f);        // viola
    public Color realWaveColor = new Color(0.6f, 0.9f, 1f, 1f);     // azzurro chiaro

    [Header("Particles")]
    public GameObject realToFantasyParticles;
    public GameObject fantasyToRealParticles;

    [Header("ShockWave")]
    public float shockWaveDuration = 1f;
    [Range(-5f, 5f)]
    public float shockWaveStrenght = -0.1f;

    [Header("PlayerLightEffect")]
    [ColorUsage(true, true)] public Color realWorldPlayerColor = new Color(0.75f, 0.9f, 1f, 1f);
    [ColorUsage(true, true)]  public Color fantasyWorldPlayerColor = new Color(0.85f, 0.75f, 1f, 1f);
    public float playerFadeInTime = 0.2f;
    public float playerFadeOutTime = 0.2f;
    [Range(0f, 1f)]
    public float MaxIntensity = 1f;

    // Inizia la transizione, gestendo particelle, effetti e cambi di mondo
    public IEnumerator PlayTransition(WorldSwitch worldSwitch)
    {
        // Determina verso quale mondo stiamo andando
        bool goingToFantasy = !worldSwitch.isFantasyWorldActive;

        // 1) Particelle
        SpawnParticles(goingToFantasy);

        // 2) Effetto glitch (after-image)
        StartCoroutine(GlitchEffect());

        // 3) Effetto luce shader sul player
        Color targetColor = goingToFantasy
            ? fantasyWorldPlayerColor
            : realWorldPlayerColor;

        LightEffect_Shader.Instance.PlayEffect(
            targetColor,
            MaxIntensity,
            playerFadeInTime,
            playerFadeOutTime
        );

        // 4) Onda principale (questa controlla anche il cambio mondo)
        yield return StartCoroutine(WaveEffect(worldSwitch, goingToFantasy));
    }

    // Crea un effetto di glitch attorno al player durante la transizione
    private IEnumerator GlitchEffect()
    {
        SpriteRenderer originalSprite = player.GetComponent<SpriteRenderer>();

        if (originalSprite == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < waveDuration)
        {
            GameObject ghost = new GameObject("PlayerGhost");

            SpriteRenderer ghostSprite = ghost.AddComponent<SpriteRenderer>();
            ghostSprite.sprite = originalSprite.sprite;
            ghostSprite.flipX = originalSprite.flipX;
            ghostSprite.sortingLayerID = originalSprite.sortingLayerID;
            ghostSprite.sortingOrder = originalSprite.sortingOrder - 1;

            ghostSprite.color = new Color(0.8f, 0f, 1f, 0.4f);

            // Piccolo offset casuale
            Vector3 offset = new Vector3(
                Random.Range(-0.15f, 0.15f),
                Random.Range(-0.05f, 0.05f),
                0f);

            ghost.transform.position = player.position + offset;
            ghost.transform.localScale = player.localScale;

            Destroy(ghost, 0.08f);

            elapsed += 0.03f;
            yield return new WaitForSeconds(0.03f);
        }
    }

    // Crea l'effetto onda che simula il passaggio tra i mondi, gestendo anche il cambio di mondo al momento giusto
    private IEnumerator WaveEffect(WorldSwitch worldSwitch, bool goingToFantasy)
    {
        GameObject selectedPrefab = goingToFantasy ? fantasyWavePrefab : realWavePrefab;
        GameObject wave = Instantiate(selectedPrefab, player.position, Quaternion.identity);

        wave.transform.localScale = Vector3.zero;

        SpriteRenderer sr = wave.GetComponent<SpriteRenderer>();

        float baseAlpha = sr != null ? sr.color.a : 1f;

        // Colore iniziale in base al mondo
        Color startColor = goingToFantasy
        ? fantasyWaveColor        // viola
        : realWaveColor;     // azzurro chiaro

        // Colore finale: LUCE PURA
        Color pureLightColor = new Color(0.95f, 0.98f, 1f, 1f);

        float elapsed = 0f;
        bool worldChanged = false;
        bool schockWaveApplied = false;
        while (elapsed < waveDuration)
        {
            // Applica shockwave una sola volta all'inizio
            if (!schockWaveApplied)
            {
                ShockWaveManager.Instance.CallShockWave(player.position, shockWaveStrenght, shockWaveDuration);
                schockWaveApplied = true;
            }
            float t = elapsed / waveDuration;

            
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

            // Cambio mondo al picco
            if (!worldChanged && t >= 0.45f)
            {
                worldSwitch.ApplyWorldChange();
                worldChanged = true;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(wave);
    }

    //Istanzia e gestisce le particelle legate al cambio mondo.
    private void SpawnParticles(bool goingToFantasy)
    {
        GameObject selectedParticles = goingToFantasy
            ? realToFantasyParticles
            : fantasyToRealParticles;

        if (selectedParticles == null)
            return;

        GameObject particles = Instantiate(selectedParticles, player.position, Quaternion.identity);

        particles.transform.SetParent(player);

        ParticleSystem ps = particles.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            ps.Play();
            Destroy(particles, ps.main.duration + ps.main.startLifetime.constantMax);
        }
    }

}
