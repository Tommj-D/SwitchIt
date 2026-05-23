using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps; // Necessario per le Tilemap

public class SecretDoorReveal : MonoBehaviour
{
    [Header("Muri Tilemap")]
    [Tooltip("Trascina qui la Tilemap del mondo reale e quella del mondo fantasy")]
    public Tilemap[] muriTilemap;

    [Header("Impostazioni Tempistiche")]
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

        // Salviamo i colori iniziali di tutte le tilemap inserite nell'array
        Color[] coloriIniziali = new Color[muriTilemap.Length];
        for (int i = 0; i < muriTilemap.Length; i++)
        {
            if (muriTilemap[i] != null)
            {
                coloriIniziali[i] = muriTilemap[i].color;
            }
        }

        float tempoTrascorso = 0f;

        // Effetto Fade-out sull'Alpha del colore di TUTTI i muri contemporaneamente
        while (tempoTrascorso < durataFade)
        {
            tempoTrascorso += Time.deltaTime;
            float nuovoAlpha = Mathf.Lerp(1f, 0f, tempoTrascorso / durataFade);
            
            for (int i = 0; i < muriTilemap.Length; i++)
            {
                if (muriTilemap[i] != null)
                {
                    muriTilemap[i].color = new Color(
                        coloriIniziali[i].r, 
                        coloriIniziali[i].g, 
                        coloriIniziali[i].b, 
                        nuovoAlpha
                    );
                }
            }
            
            yield return null; // Aspetta il frame successivo
        }

        // Spegne l'oggetto principale per disattivare definitivamente le collisioni di entrambi i muri
        gameObject.SetActive(false);
    }
}