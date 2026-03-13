using UnityEngine;

public class ParticlesToTarget : MonoBehaviour
{
    private Transform target;
    private ButtonPuzzleController controller;
    private int circleIndex;
    private float speed;
    private float arriveDistance = 0.2f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

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
        if (ps == null || ps.particleCount == 0 || target == null) return;

        if (particles == null || particles.Length < ps.particleCount)
            particles = new ParticleSystem.Particle[ps.particleCount];

        int count = ps.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            Vector3 dir = target.position - particles[i].position;
            particles[i].velocity = dir.normalized * speed;

            if (dir.magnitude < arriveDistance && !activated)
            {
                activated = true;
                controller?.ActivateCircle(circleIndex);
            }
        }

        ps.SetParticles(particles, count);
    }
}