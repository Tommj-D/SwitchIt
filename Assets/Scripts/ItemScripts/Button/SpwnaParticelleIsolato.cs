using UnityEngine;

public class SpawnaParticelleIsolato : MonoBehaviour, IButtonPuzzle
{
    [Header("Dove farle comparire? (Trascina qui l'oggetto vuoto)")]
    public Transform puntoDiSpawn;

    [Header("Le particelle da far esplodere")]
    public ParticleSystem particellePrefab;

    public void PressButton(Transform buttonPos, int index)
    {
        if (particellePrefab != null && puntoDiSpawn != null)
        {
            // Crea le particelle esattamente nel punto che hai scelto
            ParticleSystem ps = Instantiate(particellePrefab, puntoDiSpawn.position, Quaternion.identity);
            ps.Play();
            
        }
    }
}