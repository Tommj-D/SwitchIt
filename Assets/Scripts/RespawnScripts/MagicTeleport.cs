using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MagicTeleport : MonoBehaviour
{
    [Header("Impostazioni Teletrasporto")]
    public Transform destinazione; // Il Buco_Uscita
    public Image schermoNero;      // L'immagine UI
    public float durataFade = 0.5f; // Durata della transizione

    [Header("Impostazioni Fisiche")]
    public float forzaSaltoUscita = 15f; 

    [Header("Camera Confiner")]
    public CameraConfinerManager cameraConfinerManager;
    public Collider2D confinerDestinazione;

    [Header("Effetti Sonori (Opzionali)")]
    public AudioClip suonoEntrata; // Suono di caduta/risucchio
    public AudioClip suonoUscita;  // Suono di comparsa/salto

    private bool inCorso = false;
    private Vector3 scalaOriginalePlayer; // Per ricordare quanto era grande il player

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !inCorso)
        {
            StartCoroutine(SequenzaCadutaSalto(collision.gameObject));
        }
    }

    private IEnumerator SequenzaCadutaSalto(GameObject player)
    {
        inCorso = true;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        
        // Salviamo la grandezza originale del player (es. 1,1,1)
        scalaOriginalePlayer = player.transform.localScale;

        // --- 🎵 SUONO ENTRATA ---
        if (suonoEntrata != null)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(suonoEntrata);
            else
                AudioSource.PlayClipAtPoint(suonoEntrata, transform.position);
        }

        // --- FASE 1: CADUTA, RIMPICCIOLIMENTO E FADE OUT ---
        float timer = 0;
        while (timer < durataFade)
        {
            timer += Time.deltaTime;
            float avanzamento = timer / durataFade; // Va da 0 a 1 man mano che passa il tempo

            // 1. Fade dello schermo a Nero
            if (schermoNero != null)
            {
                Color c = schermoNero.color;
                c.a = Mathf.Lerp(0, 1, avanzamento);
                schermoNero.color = c;
            }

            // 2. Rimpicciolisci il Player (dalla scala originale a zero)
            player.transform.localScale = Vector3.Lerp(scalaOriginalePlayer, Vector3.zero, avanzamento);

            yield return null; // Aspetta il prossimo frame e continua a cadere
        }

        // Assicuriamoci che alla fine sia tutto nero e il player minuscolo
        if(schermoNero != null) { Color c = schermoNero.color; c.a = 1; schermoNero.color = c; }
        player.transform.localScale = Vector3.zero;

        
        // --- FASE 2: TELETRASPORTO E RESET ---
        // Sposta il giocatore
        player.transform.position = destinazione.position;
        
        // Resetta immediatamente la grandezza originale PRIMA che si veda
        player.transform.localScale = scalaOriginalePlayer;
        
        cameraConfinerManager.SetConfiner(confinerDestinazione);
        
        // Brevissima pausa per far aggiornare la camera
        yield return new WaitForSeconds(0.1f);


        // --- 🎵 SUONO USCITA ---
        if (suonoUscita != null)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(suonoUscita);
            else
                AudioSource.PlayClipAtPoint(suonoUscita, destinazione.position);
        }

        // --- FASE 3: SALTO IN USCITA E FADE IN ---
        // Applica il salto verso l'alto
        if (rb != null)
        {
            // Resetta la velocità verticale attuale per avere un salto pulito
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); 
            // Spara in alto! (ForceMode2D.Impulse è come un'esplosione istantanea)
            rb.AddForce(Vector2.up * forzaSaltoUscita, ForceMode2D.Impulse);
        }

        // Fade dello schermo per tornare trasparente
        timer = 0;
        while (timer < durataFade)
        {
            timer += Time.deltaTime;
            float avanzamento = timer / durataFade;

            if (schermoNero != null)
            {
                Color c = schermoNero.color;
                c.a = Mathf.Lerp(1, 0, avanzamento);
                schermoNero.color = c;
            }
            yield return null;
        }
        // Assicuriamoci che lo schermo sia pulito alla fine
        if(schermoNero != null) { Color c = schermoNero.color; c.a = 0; schermoNero.color = c; }

        inCorso = false;
    }
}