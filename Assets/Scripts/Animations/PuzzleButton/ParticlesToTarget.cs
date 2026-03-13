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
        if (ps == null || ps.particleCount == 0) return;

        particles = new ParticleSystem.Particle[ps.particleCount];
        int count = ps.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            Vector3 dir = target.position - particles[i].position;
            particles[i].velocity = dir.normalized * speed;

            // Se la particella è arrivata
            if (dir.magnitude < arriveDistance)
            {
                particles[i].remainingLifetime = 0f;

                // Notifica il controller **una sola volta**
                if (!activated)
                {
                    activated = true;
                    controller?.ActivateCircle(circleIndex); // usa il nuovo metodo
                }
            }
        }

        ps.SetParticles(particles, count);
    }
}