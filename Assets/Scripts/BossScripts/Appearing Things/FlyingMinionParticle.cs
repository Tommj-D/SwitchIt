using UnityEngine;

public class FlyingMinionParticle : MonoBehaviour
{
    //==================================================
    // ⚙️ PARAMETRI DI MOVIMENTO E CONFIGURAZIONE
    //==================================================
    [Header("Impostazioni Magnetismo")]
    [SerializeField] private float velocitaParticelle = 8f;
    [SerializeField] private float distanzaArrivo = 0.3f;

    [Header("Effetti e Prefab")]
    [SerializeField] private ParticleSystem burstParticle;
    [SerializeField] private GameObject minionPrefab;

    private Transform targetPoint;
    private ParticleSystem sys;
    
    // Array temporaneo necessario per memorizzare e modificare i dati di ogni singola particella
    private ParticleSystem.Particle[] particles;

    //==================================================
    // INIZIALIZZAZIONE (Chiamata dal BossManager)
    //==================================================
    public void Setup(Transform target)
    {
        targetPoint = target;
        
        // Controllo di sicurezza: se 'sys' è nullo, proviamo a prenderlo adesso
        if (sys == null)
        {
            sys = GetComponent<ParticleSystem>();
        }

        // Prepariamo l'array delle particelle
        if (sys != null)
        {
            particles = new ParticleSystem.Particle[sys.main.maxParticles];
        }
        else
        {
            Debug.LogError("ATTENZIONE: Lo script FlyingMinionParticle non ha trovato un ParticleSystem su questo GameObject!", gameObject);
        }
    }

    //==================================================
    // AGGIORNAMENTO LOGICA FRAME PER FRAME
    //==================================================
    private void Update()
    {
        // Controllo di sicurezza: se mancano i dati fondamentali, distruggiamo l'oggetto per evitare errori
        if (targetPoint == null || sys == null)
        {
            Destroy(gameObject);
            return;
        }

        // 1. Catturiamo tutte le particelle attualmente visibili nella scena e salviamo il loro numero
        int numParticlesAlive = sys.GetParticles(particles);

        // 2. Eseguiamo un ciclo "for" per analizzare e spostare ogni singola particella attiva
        for (int i = 0; i < numParticlesAlive; i++)
        {
            Vector3 particleWorldPosition;

            // Controlliamo se la simulazione della particella è impostata su Local o World
            if (sys.main.simulationSpace == ParticleSystemSimulationSpace.Local)
            {
                // Convertiamo la posizione da locale (relativa all'oggetto) a globale (mondo di gioco)
                particleWorldPosition = transform.TransformPoint(particles[i].position);
            }
            else
            {
                particleWorldPosition = particles[i].position;
            }

            // Muoviamo la posizione della particella verso il punto di arrivo finale
            particleWorldPosition = Vector3.MoveTowards(
                particleWorldPosition, 
                targetPoint.position, 
                velocitaParticelle * Time.deltaTime
            );

            // Riconvertiamo le coordinate nel formato richiesto dal Particle System
            if (sys.main.simulationSpace == ParticleSystemSimulationSpace.Local)
            {
                particles[i].position = transform.InverseTransformPoint(particleWorldPosition);
            }
            else
            {
                particles[i].position = particleWorldPosition;
            }

            // 3. Controlliamo se questa specifica particella è arrivata molto vicina al punto finale
            float distance = Vector3.Distance(particleWorldPosition, targetPoint.position);
            if (distance <= distanzaArrivo)
            {
                // Riduciamo la vita della particella sotto lo zero per farla "spegnere" immediatamente
                particles[i].remainingLifetime = -1f; 
            }
        }

        // 4. Applichiamo tutte le nuove posizioni calcolate all'interno del vero Particle System
        sys.SetParticles(particles, numParticlesAlive);

        // 5. Quando non ci sono più particelle in volo (tutte arrivate a destinazione), attiviamo lo spawn
        // Il controllo "sys.time > 0.2f" serve a evitare che lo script si distrugga appena nasce prima di emettere particelle
        if (numParticlesAlive == 0 && sys.time > 0.2f)
        {
            // Fai partire l'esplosione di burst nel punto di arrivo
            if (burstParticle != null)
            {
                Instantiate(burstParticle, targetPoint.position, Quaternion.identity);
            }

            // Spawna il minion nel punto di arrivo
            if (minionPrefab != null)
            {
                Instantiate(minionPrefab, targetPoint.position, Quaternion.identity);
            }
            
            // Distruggi il contenitore delle particelle volanti, dato che il lavoro è finito
            Destroy(gameObject);
        }
    }
}