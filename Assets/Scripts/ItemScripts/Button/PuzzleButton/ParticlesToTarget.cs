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
        if (ps == null || target == null)
            return;

        int count = ps.particleCount;
        if (count == 0)
            return;

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[count];
        ps.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            Vector3 dir = target.position - particles[i].position;

            // muove la particella verso il target
            particles[i].velocity = dir.normalized * speed;

            // se una particella arriva al target attiva il cerchio
            if (!activated && dir.magnitude < arriveDistance)
            {
                activated = true;
                controller?.ActivateCircle(circleIndex);

                // distrugge il particle system poco dopo
                Destroy(gameObject, 0.2f);
            }
        }

        ps.SetParticles(particles, count);
    }
}