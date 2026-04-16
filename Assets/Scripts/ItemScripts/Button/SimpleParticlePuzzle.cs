using System.Collections;
using UnityEngine;

public class SimpleParticlePuzzle : MonoBehaviour
{
    [Header("Oggetti")]
    public GameObject[] objectsToToggle; // oggetti da attivare/disattivare

    [Header("Particelle")]
    public ParticleSystem particlePrefab;
    public float particleSpeed = 5f;

    [Header("Timing")]
    public float delayBeforeAction = 0.3f;

    private bool isBusy = false;
    private bool state = false; // false = spento, true = acceso

    // ======== CHIAMATA DAL PULSANTE ========
    public void PressButton(Transform buttonPos, Transform target)
    {
        if (isBusy) return;

        isBusy = true;
        SpawnParticles(buttonPos, target);
    }

    // ======== PARTICELLE ========
    private void SpawnParticles(Transform start, Transform target)
    {
        ParticleSystem ps = Instantiate(particlePrefab, start.position, Quaternion.identity);

        ParticlesToTarget mover = ps.GetComponent<ParticlesToTarget>();
        if (mover != null)
            mover.Init(target, this, () => OnParticleReachedTarget(), particleSpeed);

        ps.Play();
    }

    // ======== CHIAMATO DALLE PARTICELLE QUANDO ARRIVANO ========
    public void OnParticleReachedTarget()
    {
        StartCoroutine(ExecuteAction());
    }

    // ======== LOGICA PUZZLE ========
    private IEnumerator ExecuteAction()
    {
        yield return new WaitForSeconds(delayBeforeAction);

        state = !state;

        foreach (GameObject obj in objectsToToggle)
        {
            if (obj == null) continue;

            obj.SetActive(state);
        }

        isBusy = false;
    }
}