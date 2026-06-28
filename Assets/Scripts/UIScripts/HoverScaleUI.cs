using UnityEngine;
using UnityEngine.EventSystems; // Fondamentale per i controlli del mouse sulla UI

public class HoverScaleUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Quanto deve diventare grande l'intero bottone? 1.1 = 10% più grande")]
    public float scaleMultiplier = 1.1f;

    private Vector3 originalScale;

    private void Start()
    {
        // Salviamo la grandezza iniziale di QUESTO oggetto (l'intero bottone)
        originalScale = transform.localScale;
    }

    // Si attiva quando il mouse entra nel bottone
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Ingrandisce l'intero bottone, e di conseguenza anche il testo dentro di esso
        transform.localScale = originalScale * scaleMultiplier;
    }

    // Si attiva quando il mouse esce dal bottone
    public void OnPointerExit(PointerEventData eventData)
    {
        // Ripristina la dimensione originale
        transform.localScale = originalScale;
    }
}