using UnityEngine;

public class FlyingRewardParticle : MonoBehaviour
{
    [Header("Impostazioni Magnetismo")]
    [SerializeField] private float velocitaParticelle = 8f;
    [SerializeField] private float distanzaArrivo = 0.3f;

    private Transform target;
    private RewardTarget rewardTarget;
    private ParticleSystem sys;
    
    // Array temporaneo dove salveremo i dati di ogni singola particella
    private ParticleSystem.Particle[] particles;

    public void Setup(Transform newTarget, RewardTarget newRewardTarget)
    {
        target = newTarget;
        rewardTarget = newRewardTarget;
        sys = GetComponent<ParticleSystem>();

        // Inizializziamo l'array delle particelle basandoci sulla capacità massima del sistema
        if (sys != null)
        {
            particles = new ParticleSystem.Particle[sys.main.maxParticles];
        }
    }

    private void Update()
    {
        // Controlli di sicurezza: se manca il target o il Particle System, fermiamo tutto
        if (target == null || sys == null)
        {
            Destroy(gameObject);
            return;
        }

        // 1. Recuperiamo tutte le particelle attualmente vive e visibili nella scena
        int numParticlesAlive = sys.GetParticles(particles);

        // 2. Cicliamo su ogni singola particella attiva per modificarne la posizione
        for (int i = 0; i < numParticlesAlive; i++)
        {
            // Troviamo la posizione della particella nello spazio globale (World)
            Vector3 particleWorldPosition;

            if (sys.main.simulationSpace == ParticleSystemSimulationSpace.Local)
            {
                // Se il sistema è Local, convertiamo la posizione da locale a globale
                particleWorldPosition = transform.TransformPoint(particles[i].position);
            }
            else
            {
                // Se è World, la posizione è già globale
                particleWorldPosition = particles[i].position;
            }

            // Muoviamo la posizione verso il target frame per frame
            particleWorldPosition = Vector3.MoveTowards(
                particleWorldPosition, 
                target.position, 
                velocitaParticelle * Time.deltaTime
            );

            // Riconvertiamo la posizione nel formato corretto per il Particle System
            if (sys.main.simulationSpace == ParticleSystemSimulationSpace.Local)
            {
                particles[i].position = transform.InverseTransformPoint(particleWorldPosition);
            }
            else
            {
                particles[i].position = particleWorldPosition;
            }

            // 3. Controlliamo se questa specifica particella è vicina all'obiettivo
            float distance = Vector3.Distance(particleWorldPosition, target.position);
            if (distance <= distanzaArrivo)
            {
                // Spegniamo la particella (la sua vita va a zero), simulando l'assorbimento
                particles[i].remainingLifetime = -1f; 
            }
        }

        // 4. Applichiamo le modifiche apportate alle particelle nell'array di ritorno al sistema
        sys.SetParticles(particles, numParticlesAlive);

        // 5. Se non ci sono più particelle vive (sono tutte arrivate a destinazione), attiviamo la ricompensa
        if (numParticlesAlive == 0 && sys.time > 0.2f)
        {
            if (rewardTarget != null)
            {
                rewardTarget.Reveal();
            }
            
            // Distruggiamo l'oggetto contenitore
            Destroy(gameObject);
        }
    }
}