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

    [Header("Prefab particelle")]
    public ParticleSystem particlePrefab;

    [Header("Colori")]
    public Color glowColor = Color.yellow;

    private bool[] circleActivated;
    private bool hasCircles;

    private void Start()
    {
        hasCircles = circles != null && circles.Length > 0;
        if (hasCircles)
            circleActivated = new bool[circles.Length];

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
                if (!circleActivated[i])
                {
                    SpawnParticles(buttonPos, circles[i].transform, i);
                    break;
                }
            }
        }
        else
        {
            // Nessun cerchio: attiva/disattiva subito oggetti
            foreach (GameObject obj in objectsToHide)
                if (obj != null) obj.SetActive(false);

            foreach (GameObject obj in objectsToShow)
                if (obj != null) obj.SetActive(true);
        }
    }

    // Chiamato da ParticlesToTarget quando le particelle arrivano
    public void ActivateCircle(int index)
    {
        if (!hasCircles || circleActivated[index]) return;

        circleActivated[index] = true;

        // Illumina il glow del cerchio
        SpriteRenderer glow = circles[index].transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (glow != null)
            glow.color = glowColor;

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
        if (hasCircles)
        {
            float duration = 1f;
            float timer = 0f;

            SpriteRenderer[] srs = new SpriteRenderer[objectsToHide.Length];

            for (int i = 0; i < objectsToHide.Length; i++)
            {
                if (objectsToHide[i] != null)
                    srs[i] = objectsToHide[i].GetComponent<SpriteRenderer>();
            }

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, timer / duration);

                for (int i = 0; i < objectsToHide.Length; i++)
                {
                    if (srs[i] != null)
                    {
                        Color c = srs[i].color;
                        c.a = alpha;
                        srs[i].color = c;
                    }
                }

                yield return null;
            }
        }

        // Alla fine, nascondi/mostra oggetti
        foreach (GameObject obj in objectsToHide)
            if (obj != null) obj.SetActive(false);

        foreach (GameObject obj in objectsToShow)
            if (obj != null) obj.SetActive(true);
    }

    private void SpawnParticles(Transform start, Transform target, int circleIndex)
    {
        if (!hasCircles) return;

        ParticleSystem ps = Instantiate(particlePrefab, start.position, Quaternion.identity);
        ParticlesToTarget mover = ps.GetComponent<ParticlesToTarget>();
        if (mover != null)
        {
            mover.target = target;
            mover.controller = this;
            mover.circleIndex = circleIndex;
        }
        ps.Play();
    }
}