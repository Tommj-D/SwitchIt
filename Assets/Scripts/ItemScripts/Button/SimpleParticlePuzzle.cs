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

    private bool isBusy = false;
    private bool puzzleSolved = false;

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
            if (obj != null) obj.SetActive(true);

        // SOUND
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.glowingSound);

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
}