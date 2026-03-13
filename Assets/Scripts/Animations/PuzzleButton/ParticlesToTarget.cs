using UnityEngine;

public class ParticlesToTarget : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;
    public float arriveDistance = 0.2f;

    public ButtonPuzzleController controller;
    public int circleIndex;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    private bool activated = false;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void LateUpdate()
    {
        int count = ps.GetParticles(particles = new ParticleSystem.Particle[ps.particleCount]);

        for (int i = 0; i < count; i++)
        {
            Vector3 dir = target.position - particles[i].position;

            particles[i].velocity = dir.normalized * speed;

            if (dir.magnitude < arriveDistance)
            {
                particles[i].remainingLifetime = 0f;

                if (!activated)
                {
                    activated = true;

                    controller.IlluminateCircle(circleIndex);
                }
            }
        }

        ps.SetParticles(particles, count);
    }
}