using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ButtonPuzzleController : MonoBehaviour
{
    [Header("Cerchi da illuminare")]
    public SpriteRenderer[] circles;

    [Header("Oggetti da nascondere")]
    public GameObject[] objectsToHide;

    [Header("Oggetti da mostrare")]
    public GameObject[] objectsToShow;

    [Header("Particelle")]
    public ParticleSystem particlePrefab;
    public float particleSpeed = 5f;

    [Header("Colori")]
    public Color int_glowColor = Color.yellow;
    [ColorUsage(true, true)]
    public Color ext_glowColor = Color.yellow;

    private bool[] circleActivated;
    private bool[] circleReserved;
    private bool hasCircles;

    private void Start()
    {
        hasCircles = circles != null && circles.Length > 0;
        if (hasCircles) {
            circleActivated = new bool[circles.Length];
            circleReserved = new bool[circles.Length];
        }

        // Assicurati che gli oggetti da mostrare siano inizialmente invisibili
        foreach (GameObject obj in objectsToShow)
            if (obj != null) obj.SetActive(false);
    }

    // Chiamato dal pulsante
    public void ButtonPressed(Transform buttonPos)
    {
        if (hasCircles)
        {
            // Solo il primo cerchio non attivo riceve particelle
            for (int i = 0; i < circles.Length; i++)
            {
                if (!circleActivated[i] && !circleReserved[i])
                {
                    circleReserved[i] = true; // prenota il cerchio subito
                    SpawnParticles(buttonPos, circles[i].transform, i);
                    break;
                }
            }
        }
        else
        {
            // Nessun cerchio: attiva/disattiva subito oggetti
            foreach (GameObject obj in objectsToHide)
            {
                if (obj != null)
                {
                    Dissolve d = obj.GetComponent<Dissolve>();

                    if (d != null)
                        d.DissolveObject();
                    else
                        obj.SetActive(false);
                }
            }

            foreach (GameObject obj in objectsToShow)
            {
                if (obj != null)
                {
                    obj.SetActive(true);

                    Dissolve d = obj.GetComponent<Dissolve>();

                    if (d != null)
                        d.AppearObject();
                }
            }
        }
    }

    // Chiamato da ParticlesToTarget quando le particelle arrivano
    public void ActivateCircle(int index)
    {
        if (!hasCircles || circleActivated[index]) return;

        circleActivated[index] = true;
        circleReserved[index] = false;

        // Illumina il glow interno (figlio)
        SpriteRenderer glow = circles[index].transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (glow != null)
        {
            glow.color = int_glowColor;
        }

        // Illumina lo shader esterno (materiale)
        Renderer circleRenderer = circles[index].GetComponent<Renderer>();
        if (circleRenderer != null)
        {
            // crea una copia del materiale per non modificare il materiale condiviso
            Material mat = new Material(circleRenderer.material);
            mat.SetColor("_HitEffectColor", ext_glowColor);
            circleRenderer.material = mat;
        }

        // Se ha Animator, attiva trigger
        Animator anim = circles[index].GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("Active");

        // Se tutti i cerchi sono attivi, parte la routine di nascondere/mostrare
        if (AllCirclesActive())
            StartCoroutine(HideAndShowObjects());
    }

    private bool AllCirclesActive()
    {
        if (!hasCircles) return true;

        foreach (bool b in circleActivated)
            if (!b) return false;
        return true;
    }

    private IEnumerator HideAndShowObjects()
    {
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
            {
                Dissolve d = obj.GetComponent<Dissolve>();

                if (d != null)
                    d.DissolveObject();
                else
                    obj.SetActive(false);
            }
        }

        yield return new WaitForSeconds(0.8f);

        foreach (GameObject obj in objectsToShow)
        {
            if (obj != null)
            {
                obj.SetActive(true);

                Dissolve d = obj.GetComponent<Dissolve>();

                if (d != null)
                    d.AppearObject();
            }
        }
    }

    private void SpawnParticles(Transform start, Transform target, int circleIndex)
    {
        if (!hasCircles) return;

        // Istanzia prefab particelle
        ParticleSystem ps = Instantiate(particlePrefab, start.position, Quaternion.identity);

        // Inizializza il prefab con tutti i dati necessari
        ParticlesToTarget mover = ps.GetComponent<ParticlesToTarget>();
        if (mover != null)
        {
            mover.Init(target, this, circleIndex, particleSpeed); // particleSpeed � ora un campo del controller
        }

        ps.Play();
    }
}