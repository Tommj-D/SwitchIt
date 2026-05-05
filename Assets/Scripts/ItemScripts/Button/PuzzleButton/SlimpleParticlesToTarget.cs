using UnityEngine;

public class ParticleSystemToTarget : MonoBehaviour
{
    public float speed = 5f;
    public float attractionForce = 15f;
    public float arriveDistance = 0.2f;

    [Range(0f, 1f)]
    public float completionThreshold = 0.9f;

    public GameObject burstPrefab;

    private Vector3 target;
    private System.Action onComplete;
    private ParticleSystem ps;
    private bool done = false;

    public void Init(Vector3 targetPos, System.Action callback)
    {
        target = targetPos;
        onComplete = callback;
    }

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void LateUpdate()
    {
        if (ps == null || done) return;

        int count = ps.particleCount;
        if (count == 0) return;

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[count];
        ps.GetParticles(particles);

        int arrived = 0;

        for (int i = 0; i < count; i++)
        {
            Vector3 dir = target - particles[i].position;
            float dist = dir.magnitude;

            // movimento base
            Vector3 velocity = dir.normalized * speed;

            // 🔥 attrazione forte vicino al centro
            velocity += dir.normalized * (attractionForce / Mathf.Max(dist, 0.1f));

            particles[i].velocity = velocity;

            if (dist < arriveDistance)
            {
                particles[i].remainingLifetime = 0f;
                arrived++;
            }
        }

        ps.SetParticles(particles, count);

        float completion = (float)arrived / count;

        if (!done && completion >= completionThreshold)
        {
            done = true;

            // 💥 burst
            if (burstPrefab != null)
                Instantiate(burstPrefab, target, Quaternion.identity);

            // 🟢 slime
            onComplete?.Invoke();

            ps.Stop();
            Destroy(gameObject, 0.3f);
        }
    }
}