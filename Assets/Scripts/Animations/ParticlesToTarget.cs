using UnityEngine;

public class ParticlesToTarget : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void LateUpdate()
    {
        int count = ps.GetParticles(particles = new ParticleSystem.Particle[ps.particleCount]);

        for (int i = 0; i < count; i++)
        {
            Vector3 dir = (target.position - particles[i].position).normalized;
            particles[i].velocity = dir * speed;
        }

        ps.SetParticles(particles, count);
    }
}