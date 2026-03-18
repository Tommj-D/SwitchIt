using UnityEngine;

public class ParticlesToTarget : MonoBehaviour
{
    private Transform target;
    private ButtonPuzzleController controller;
    private int circleIndex;

    private float speed;
    private float arriveDistance;

    private ParticleSystem ps;
    private bool activated = false;

    [Range(0f, 1f)]
    public float completionThreshold = 0.9f; // soglia 90%

    // Inizializzazione dal controller
    public void Init(Transform target, ButtonPuzzleController controller, int circleIndex, float speed, float arriveDistance = 0.2f)
    {
        this.target = target;
        this.controller = controller;
        this.circleIndex = circleIndex;
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

            // base velocity verso target
            particles[i].velocity = dir.normalized * speed;

            // attrazione morbida quando vicini al centro
            if (distance < arriveDistance * 5f)
            {
                // aggiunge un piccolo “pull” verso il centro
                particles[i].velocity += dir.normalized * (speed * 0.5f * (1f - distance / (arriveDistance * 5f)));
            }

            // check arrivo
            if (distance <= arriveDistance)
                arrivedCount++;
        }

        // calcola la percentuale di completamento
        float completion = (float)arrivedCount / count;

        if (!activated && completion >= completionThreshold)
        {
            activated = true;
            controller?.ActivateCircle(circleIndex);

            // stoppa emissione e distrugge dopo breve
            ps.Stop();
            Destroy(gameObject, 0.2f);
        }

        ps.SetParticles(particles, count);
    }
}