using System.Collections;
using UnityEngine;

public class SimpleParticlePuzzle : MonoBehaviour, IButtonPuzzle
{
    [Header("Target unico")]
    public Transform target;

    [Header("Oggetti")]
    public GameObject[] objectsToHide;
    public GameObject[] objectsToShow;

    [Header("Particelle")]
    public ParticleSystem particle_Real;
    public ParticleSystem particle_Fantasy;
    public float particleSpeed = 5f;

    [Header("Burst")]
    public ParticleSystem burstReal;
    public ParticleSystem burstFantasy;

    [Header("Timing")]
    public float delayBeforeAction = 0.2f;

    [Header("Fade")]
    public float fadeDuration = 0.5f;

    [Header("Audio")]
    public float volumeSFX = 1f;

    private bool isBusy = false;
    private bool puzzleSolved = false;

    private void Awake()
    {
        // Disattiva tutti gli oggetti da mostrare all'inizio
        foreach (GameObject obj in objectsToShow)
            if (obj != null) obj.SetActive(false);
    }

    // ======== INPUT ========
    public void PressButton(Transform buttonPos, int index)
    {
        if (isBusy || puzzleSolved) return;

        if (target == null)
        {
            Debug.LogWarning("Target non assegnato!");
            return;
        }

        isBusy = true;
        SpawnParticles(buttonPos, target);
    }

    // ======== PARTICELLE ========
    private void SpawnParticles(Transform start, Transform target)
    {
        ParticleSystem ps = WorldSwitch.Instance.isFantasyWorldActive
            ? Instantiate(particle_Fantasy, start.position, Quaternion.identity)
            : Instantiate(particle_Real, start.position, Quaternion.identity);

        ParticlesToTarget mover = ps.GetComponent<ParticlesToTarget>();
        if (mover != null)
            mover.Init(target, this, () => OnParticleReachedTarget(), particleSpeed);

        ps.Play();
    }

    // ======== CALLBACK ========
    public void OnParticleReachedTarget()
    {
        StartCoroutine(ExecuteAction());
    }

    // ======== LOGICA ========
    private IEnumerator ExecuteAction()
    {
        if (puzzleSolved)
            yield break;

        yield return new WaitForSeconds(delayBeforeAction);

        puzzleSolved = true;

        foreach (GameObject obj in objectsToHide)
            if (obj != null) obj.SetActive(false);

        foreach (GameObject obj in objectsToShow)
        {
            if (obj == null) continue;

            obj.SetActive(true);

            // Se contiene particelle niente fade
            if (HasParticles(obj))
            {
                SetAlphaInstant(obj, 1f);
                continue;
            }

            // Se è attivo davvero nella scena fade
            if (obj.activeInHierarchy)
            {
                StartCoroutine(FadeInObject(obj));
            }
            else
            {
                // È in un mondo disattivo  niente fade
                SetAlphaInstant(obj, 1f);
            }
        }

        // SOUND
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.particlesArrivalSound, volumeSFX);

        // BURST
        ParticleSystem burstPrefab = WorldSwitch.Instance.isFantasyWorldActive
            ? burstFantasy
            : burstReal;

        if (burstPrefab != null)
        {
            ParticleSystem burst = Instantiate(burstPrefab, transform.position, Quaternion.identity);
            burst.Play();
            Destroy(burst.gameObject, 2f);
        }

        isBusy = false;
    }

    //Per il fade IN
    private IEnumerator FadeInObject(GameObject obj)
    {
        float t = 0f;

        SetAlphaInstant(obj, 0f);

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = t / fadeDuration;

            SetAlphaInstant(obj, alpha);

            yield return null;
        }

        SetAlphaInstant(obj, 1f);
    }

    private void SetAlphaInstant(GameObject obj, float alpha)
    {
        // SpriteRenderer
        foreach (var sr in obj.GetComponentsInChildren<SpriteRenderer>())
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        // Tilemap
        foreach (var tile in obj.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>())
        {
            Color c = tile.color;
            c.a = alpha;
            tile.color = c;
        }
    }

    private bool HasParticles(GameObject obj)
    {
        return obj.GetComponentInChildren<ParticleSystem>() != null;
    }
}