using UnityEngine;
using System.Collections;

// Questo assicura che lo script trovi un componente grafico (Sprite o Tilemap)
[RequireComponent(typeof(Renderer))] 
public class WallFadeIn : MonoBehaviour
{
    [Header("Impostazioni Dissolvenza")]
    [Tooltip("Quanti secondi aspettare prima di iniziare ad apparire?")]
    public float ritardoIniziale = 2f;
    
    [Tooltip("Quanto tempo ci mette il muro a passare da trasparente a visibile?")]
    public float durataDissolvenza = 1f;

    private Renderer renderComponent;
    private Collider2D col;
    private Color coloreOriginale;

    private void Awake()
    {
        renderComponent = GetComponent<Renderer>();
        col = GetComponent<Collider2D>();
        
        // Salviamo il colore originale (ci serve per sapere l'esatto colore e opacità finale)
        if (renderComponent != null && renderComponent.material != null)
        {
            coloreOriginale = renderComponent.material.color;
        }
    }

    // OnEnable scatta in automatico appena l'oggetto viene acceso (SetActive(true))
    private void OnEnable()
    {
        StartCoroutine(AppariConDissolvenza());
    }

    private IEnumerator AppariConDissolvenza()
    {
        // 1. Appena acceso, rendiamolo subito totalmente trasparente
        Color coloreAttuale = coloreOriginale;
        coloreAttuale.a = 0f; 
        renderComponent.material.color = coloreAttuale;

        // Disattiviamo la collisione durante l'attesa (così non ci sbatti contro mentre non c'è)
        if (col != null) col.enabled = false;

        // 2. Aspettiamo i secondi di ritardo
        yield return new WaitForSeconds(ritardoIniziale);

        // 3. Attiviamo la collisione (il muro è chiuso, il player non scappa più!)
        if (col != null) col.enabled = true;

        // 4. Iniziamo il Fade In visivo
        float timer = 0f;
        while (timer < durataDissolvenza)
        {
            timer += Time.deltaTime;
            // Lerp calcola gradualmente il passaggio da 0 (trasparente) a 1 (solido)
            coloreAttuale.a = Mathf.Lerp(0f, 1f, timer / durataDissolvenza);
            renderComponent.material.color = coloreAttuale;
            
            yield return null;
        }

        // 5. Assicuriamoci che alla fine sia visibile al 100%
        coloreAttuale.a = 1f;
        renderComponent.material.color = coloreAttuale;
    }
}