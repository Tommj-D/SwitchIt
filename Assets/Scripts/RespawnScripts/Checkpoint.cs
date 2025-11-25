using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    public AddColorEffect colorEffect;

    public Color activatedRuneColor = Color.green;
    public Color activatedParticlesColor = Color.green;
    public float activatedGlowStrength = 1.5f;
    public float activatedParticlesStrength = 1.5f;

    public float transitionDuration = 0.8f;

    private bool activated = false;


    private void Start()
    {
        // se non assegnato nell'inspector, prova a cercarlo nei figli/genitore
        if (colorEffect == null)
        {
            colorEffect = GetComponentInChildren<AddColorEffect>();
            if (colorEffect == null)
                colorEffect = GetComponentInParent<AddColorEffect>();
        }

        if (colorEffect == null)
        {
            Debug.LogWarning($"Checkpoint ({name}): AddColorEffect non trovato. Assegna il riferimento nell'Inspector se vuoi cambiare i colori.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return; //evita riattivazioni

        if (!other.CompareTag("Player")) return;

        PlayerRespawn playerRespawn = other.GetComponent<PlayerRespawn>();
        if (playerRespawn != null)
        {
            playerRespawn.respawnPoint = transform.position;
        }

        // Se la transizione è 0 o negativa: applica subito i valori
        if (transitionDuration <= 0f)
        {
            ApplyActivatedValuesInstant();
            activated = true;
        }
        else
        {
            // marca subito come attivato per evitare doppie chiamate durante il lerp
            activated = true;
            StopAllCoroutines();
            StartCoroutine(LerpToActivated(transitionDuration));
        }
    }

    private void ApplyActivatedValuesInstant()
    {
        colorEffect.runeColor = activatedRuneColor;
        colorEffect.particlesColor = activatedParticlesColor;
        colorEffect.glowStrength = activatedGlowStrength;
        colorEffect.particlesStrength = activatedParticlesStrength;
    }

    private IEnumerator LerpToActivated(float duration)
    {
        // valori iniziali (li prendiamo direttamente dallo script AddColorEffect)
        Color startRune = colorEffect.runeColor;
        Color startParticles = colorEffect.particlesColor;
        float startGlow = colorEffect.glowStrength;
        float startParticlesStrength = colorEffect.particlesStrength;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            // easing (smoothstep): più gradevole della lerp lineare
            float ease = k * k * (3f - 2f * k);

            colorEffect.runeColor = Color.Lerp(startRune, activatedRuneColor, ease);
            colorEffect.particlesColor = Color.Lerp(startParticles, activatedParticlesColor, ease);
            colorEffect.glowStrength = Mathf.Lerp(startGlow, activatedGlowStrength, ease);
            colorEffect.particlesStrength = Mathf.Lerp(startParticlesStrength, activatedParticlesStrength, ease);

            yield return null;
        }

        // assicuriamoci che siano i valori finali esatti
        ApplyActivatedValuesInstant();
    }
}
