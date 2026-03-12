using UnityEngine;

public class ParticlesToTarget : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;
    public float arriveDistance = 0.2f;

    public ButtonPuzzleController controller;
    public int circleIndex;

    [Header("Colore Target")]
    public Color targetColor = Color.yellow;

    private SpriteRenderer targetSprite;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    private bool activated = false;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();

        if (target != null)
            targetSprite = target.GetComponentInChildren<SpriteRenderer>();
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

                    if (targetSprite != null)
                        targetSprite.color = targetColor;

                    controller.IlluminateCircle(circleIndex);
                }
            }
        }

        ps.SetParticles(particles, count);
    }
}