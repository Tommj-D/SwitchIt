using UnityEngine;

public class ChangeCircleMaterial : MonoBehaviour
{
    public Material dissolveMaterial;

    // Passa l'array dei cerchi principali, ma la funzione cambierà anche tutti i figli
    public void SwitchMaterial(SpriteRenderer[] circles)
    {
        foreach (SpriteRenderer circle in circles)
        {
            // Prendi tutti i figli (incluso l'oggetto stesso) con SpriteRenderer
            SpriteRenderer[] renderers = circle.GetComponentsInChildren<SpriteRenderer>(true);

            foreach (SpriteRenderer sr in renderers)
            {
                sr.material = new Material(dissolveMaterial); // crea una copia per sicurezza
            }
        }
    }
}