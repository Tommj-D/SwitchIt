using UnityEngine;
using System.Collections;

public class SecretDoorReveal : MonoBehaviour
{
    [Header("Effetti Visivi e Sonori")]
    [Tooltip("Trascina qui un Particle System per l'effetto polvere/esplosione rocce (Opzionale)")]
    public ParticleSystem esplosioneRocce;
    
    [Tooltip("Il suono del muro che si frantuma (Opzionale)")]
    public AudioClip suonoCrollo;
    
    [Tooltip("Quanti secondi aspettare dopo la morte del boss prima di far crollare il muro?")]
    public float ritardoPrimaDelCrollo = 2f;

    public void ApriPassaggioSegreto()
    {
        StartCoroutine(SequenzaCrollo());
    }

    private IEnumerator SequenzaCrollo()
    {
        // 1. Attesa drammatica dopo la morte del boss
        yield return new WaitForSeconds(ritardoPrimaDelCrollo);

        // 2. Audio del crollo
        if (AudioManager.Instance != null && suonoCrollo != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(suonoCrollo);
        }

        // 3. Particelle
        if (esplosioneRocce != null)
        {
            esplosioneRocce.Play();
        }

        // 4. SPEGNE L'OGGETTO (Così sparisce sia la grafica che il muro invisibile!)
        gameObject.SetActive(false);
    }
}