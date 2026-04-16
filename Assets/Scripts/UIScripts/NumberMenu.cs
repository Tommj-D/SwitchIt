using UnityEngine;
using UnityEngine.EventSystems;

public class NumberMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator anim;
    private UnityEngine.UI.Button mioBottone; 

    void Start()
    {
        anim = GetComponent<Animator>();
        mioBottone = GetComponent<UnityEngine.UI.Button>(); 

        if (anim != null) 
        {
            anim.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Se il livello è sbloccato, inizia a saltare
        if (mioBottone != null && mioBottone.interactable == true && anim != null)
        {
            anim.enabled = true; 
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (anim != null)
        {
            // IL TRUCCO MAGICO: Riavvolge l'animazione al punto zero perfetto!
            anim.Rebind(); 
            anim.Update(0f); 
            
            anim.enabled = false; // Spegne il motore dell'animazione
        }
    }
}