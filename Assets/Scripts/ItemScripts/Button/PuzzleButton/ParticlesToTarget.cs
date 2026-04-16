using UnityEngine;

public class ParticlesToTarget : MonoBehaviour
{
    private MonoBehaviour controller;
    private System.Action onComplete;   
    private Transform target;
    private int circleIndex;

    private float speed;
    private float arriveDistance;

    private ParticleSystem ps;
    private bool activated = false;

    [Range(0f, 1f)]
    public float completionThreshold = 0.9f; // soglia 90%

    public float ArriveDistanceMultiplier = 10f; // quando inizia il vortice
    public float AttractionForceMultiplier = 0.5f; // forza attrazione finale
    public float VortexForceMultiplier = 0.5f;     // forza vortice

    // Inizializzazione dal controller
    public void Init(Transform target, MonoBehaviour controller, System.Action onComplete, float speed, float arriveDistance = 0.2f)
    {
        this.target = target;
        this.controller = controller;
        this.onComplete = onComplete;
        this.speed = speed;
        this.arriveDistance = arriveDistance;
    }

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void LateUpdate()
    {
        if (ps == null || target == null || activated)
            return;

        int count = ps.particleCount;
        if (count == 0)
            return;

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[count];
        ps.GetParticles(particles);

        int arrivedCount = 0;

        for (int i = 0; i < count; i++)
        {
            Vector3 dir = target.position - particles[i].position;
            float distance = dir.magnitude;

            // base velocity verso target (sempre)
            particles[i].velocity = dir.normalized * speed;

            // quando sono abbastanza vicine, aggiungi vortice + attrazione finale
            if (distance < arriveDistance * ArriveDistanceMultiplier)
            {
                // attrazione morbida extra
                particles[i].velocity += dir.normalized * (speed * AttractionForceMultiplier * (1f - distance / (arriveDistance * ArriveDistanceMultiplier)));

                // vortice solo vicino
                Vector3 perpendicular = new Vector3(-dir.y, dir.x, 0f);
                particles[i].velocity += perpendicular * (speed * VortexForceMultiplier);
            }

            // scomparsa finale
            if (distance <= arriveDistance)
            {
                particles[i].remainingLifetime = 0f;
                arrivedCount++;
            }
        }

        // calcola la percentuale di completamento
        float completion = (float)arrivedCount / count;

        if (!activated && completion >= completionThreshold)
        {
            activated = true;
            onComplete?.Invoke();

            // stoppa emissione e distrugge dopo breve
            ps.Stop();
            Destroy(gameObject, 0.2f);
        }

        ps.SetParticles(particles, count);
    }
}