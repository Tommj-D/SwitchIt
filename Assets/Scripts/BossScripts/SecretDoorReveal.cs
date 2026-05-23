using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps; // Necessario per usare la Tilemap

public class SecretDoorReveal : MonoBehaviour
{
    [Tooltip("Quanti secondi aspettare dopo la morte del boss prima di iniziare il fade del muro?")]
    public float ritardoPrimaDelCrollo = 2f;
    
    [Tooltip("Durata in secondi dell'effetto di fade (scomparsa graduale).")]
    public float durataFade = 1.5f;

    public void ApriPassaggioSegreto()
    {
        StartCoroutine(SequenzaCrollo());
    }

    private IEnumerator SequenzaCrollo()
    {
        // Attesa drammatica dopo la morte del boss
        yield return new WaitForSeconds(ritardoPrimaDelCrollo);

        // Recuperiamo la Tilemap attaccata a questo GameObject
        Tilemap tilemap = GetComponent<Tilemap>();
        
        if (tilemap != null)
        {
            Color coloreIniziale = tilemap.color;
            float tempoTrascorso = 0f;

            // Effetto Fade-out sull'Alpha del colore
            while (tempoTrascorso < durataFade)
            {
                tempoTrascorso += Time.deltaTime;
                float nuovoAlpha = Mathf.Lerp(1f, 0f, tempoTrascorso / durataFade);
                
                tilemap.color = new Color(coloreIniziale.r, coloreIniziale.g, coloreIniziale.b, nuovoAlpha);
                
                yield return null; // Aspetta il frame successivo
            }
        }
        else
        {
            Debug.LogWarning("Nessuna Tilemap trovata su " + gameObject.name + " per fare il fade!");
        }

        // SPEGNE L'OGGETTO definitivamente (rimuove collisioni etc.)
        gameObject.SetActive(false);
    }
}