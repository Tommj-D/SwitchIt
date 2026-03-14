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
    public ParticleSystem particle_Real;
    public ParticleSystem particle_Fantasy;
    public ParticleSystem circleBurst;
    public float particleSpeed = 5f;

    [Header("Colori Cerchi")]
    [Header("Disattivati")]
    public Color int_idleColor_Real = Color.gray;
    public Color int_idleColor_Fantasy = Color.cyan;
    [Header("Attivati")]
    public Color int_glowColor_Real = Color.yellow;
    public Color int_glowColor_Fantasy = Color.cyan;
    [ColorUsage(true, true)]
    public Color ext_glowColor_Real = Color.yellow;
    [ColorUsage(true, true)]
    public Color ext_glowColor_Fantasy = Color.cyan;

    [Header("Volumi Dissolvenza/Comparsa")]
    [Range(0f, 1f)] public float dissolve_volume = 1f;
    [Range(0f, 1f)] public float dissolve_pitch = 1f;

    [Header("Puzzle Timing")]
    [Header("Puzzle Timing")]
    [Range(0f, 5f)] public float delayBeforeDissolve = 0.2f;

    public ChangeCircleMaterial circleMaterialChanger;

    private bool[] circleActivated;
    private bool[] circleReserved;
    private bool hasCircles;

    private bool puzzleSolved = false;

    private void Start()
    {
        hasCircles = circles != null && circles.Length > 0;
        if (hasCircles) {
            circleActivated = new bool[circles.Length];
            circleReserved = new bool[circles.Length];
        }

        UpdateCircleColors();

        // Assicurati che gli oggetti da mostrare siano inizialmente invisibili
        foreach (GameObject obj in objectsToShow)
            if (obj != null) obj.SetActive(false);
    }

    // Chiamato dal pulsante
    public void ButtonPressed(Transform buttonPos)
    {
        if (puzzleSolved) return;

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

        if (circleBurst != null)
        {
            //Faccio partire audio glowing
            AudioManager.Instance.PlaySFX(AudioManager.Instance.glowingSound);

            Vector3 pos = circles[index].transform.position;
            pos += Random.insideUnitSphere * 0.05f;

            ParticleSystem burst = Instantiate(circleBurst, pos, Quaternion.identity);

            burst.Play();
            Destroy(burst.gameObject, 2f); // distrugge dopo che ha finito
        }

        // Illumina il glow interno (figlio)
        SpriteRenderer glow = circles[index].transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (glow != null)
        {
            glow.color = WorldSwitch.Instance.isFantasyWorldActive ? int_glowColor_Fantasy : int_glowColor_Real;
        }

        // Illumina lo shader esterno (materiale)
        Renderer circleRenderer = circles[index].GetComponent<Renderer>();
        if (circleRenderer != null)
        {
            // crea una copia del materiale per non modificare il materiale condiviso
            Material mat = new Material(circleRenderer.material);
            mat.SetColor("_HitEffectColor", WorldSwitch.Instance.isFantasyWorldActive ? ext_glowColor_Fantasy : ext_glowColor_Real);
            circleRenderer.material = mat;
        }

        // Se ha Animator, attiva trigger
        Animator anim = circles[index].GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("Active");

        // Se tutti i cerchi sono attivi, parte la routine di nascondere/mostrare
        if (AllCirclesActive())
        {
            puzzleSolved = true;

            StartCoroutine(HideAndShowObjects());
        }
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
        // Aspetta prima di far partire la dissolvenza
        yield return new WaitForSeconds(delayBeforeDissolve);

        //Cambio materiale in modo che possa dissolversi correttamente
        if (circleMaterialChanger != null)
            circleMaterialChanger.SwitchMaterial(circles);

        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
            {
                if (obj.activeInHierarchy)
                {
                    //Faccio partire audio wall disappearing
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.wallDisappearingSound, dissolve_volume, dissolve_pitch);

                    Dissolve d = obj.GetComponent<Dissolve>();

                    if (d != null)
                    {
                        d.RefreshRenderers();
                        d.DissolveObject();
                    }
                    else
                        obj.SetActive(false);
                }
                else
                {
                    // se è nel mondo inattivo lo disattiviamo comunque
                    obj.SetActive(false);
                }
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
        ParticleSystem ps;

        if (WorldSwitch.Instance.isFantasyWorldActive)
            ps = Instantiate(particle_Fantasy, start.position, Quaternion.identity);
        else
            ps = Instantiate(particle_Real, start.position, Quaternion.identity);

        // Inizializza il prefab con tutti i dati necessari
        ParticlesToTarget mover = ps.GetComponent<ParticlesToTarget>();
        if (mover != null)
        {
            mover.Init(target, this, circleIndex, particleSpeed); // particleSpeed è ora un campo del controller
        }

        ps.Play();
    }

    public void UpdateCircleColors()
    {
        bool fantasy = WorldSwitch.Instance.isFantasyWorldActive;

        for (int i = 0; i < circles.Length; i++)
        {
            bool active = circleActivated[i];

            Color intColor;
            Color extColor;

            if (active)
            {
                intColor = fantasy ? int_glowColor_Fantasy : int_glowColor_Real;
                extColor = fantasy ? ext_glowColor_Fantasy : ext_glowColor_Real;
            }
            else
            {
                intColor = fantasy ? int_idleColor_Fantasy : int_idleColor_Real;
                extColor = Color.black; // oppure nessun glow
            }

            // glow interno
            SpriteRenderer glow = circles[i].transform.GetChild(0).GetComponent<SpriteRenderer>();
            if (glow != null)
                glow.color = intColor;

            // glow shader esterno
            Renderer circleRenderer = circles[i].GetComponent<Renderer>();
            if (circleRenderer != null)
            {
                circleRenderer.material.SetColor("_HitEffectColor", extColor);
            }
        }
    }
}