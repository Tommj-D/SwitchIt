using System.Collections;
using UnityEngine;

public class ButtonPuzzleController : MonoBehaviour
{
    // ======== RIFERIMENTI A OGGETTI NELLA SCENA ========
    [Header("Cerchi da illuminare")]
    public SpriteRenderer[] circles; // Array dei cerchi da illuminare

    [Header("Oggetti da nascondere")]
    public GameObject[] objectsToHide; // Oggetti che spariranno quando il puzzle è risolto

    [Header("Oggetti da mostrare")]
    public GameObject[] objectsToShow; // Oggetti che appariranno quando il puzzle è risolto

    // ======== PARTICELLE ========
    [Header("Particelle")]
    public ParticleSystem particle_Real; // Particelle mondo reale
    public ParticleSystem particle_Fantasy; // Particelle mondo fantasy
    public float particleSpeed = 5f; // Velocità delle particelle verso il cerchio
    [Header("Particelle Burst Cerchi")]
    public ParticleSystem circleBurst_Real;
    public ParticleSystem circleBurst_Fantasy;

    // ======== COLORI CERCHI ========
    [Header("Colori Cerchi")]
    [Header("Disattivati")]
    public Color int_idleColor_Real = Color.gray; // Glow interno cerchio spento, mondo reale
    public Color int_idleColor_Fantasy = Color.cyan; // Glow interno cerchio spento, mondo fantasy
    [Header("Attivati")]
    public Color int_glowColor_Real = Color.yellow; // Glow interno cerchio attivo, mondo reale
    public Color int_glowColor_Fantasy = Color.cyan; // Glow interno cerchio attivo, mondo fantasy
    [ColorUsage(true, true)]
    public Color ext_glowColor_Real = Color.yellow; // Glow esterno cerchio attivo, mondo reale
    [ColorUsage(true, true)]
    public Color ext_glowColor_Fantasy = Color.cyan; // Glow esterno cerchio attivo, mondo fantasy

    // ======== PARAMETRI AUDIO / DISSOLVENZA ========
    [Header("Volumi Dissolvenza/Comparsa")]
    [Range(0f, 1f)] public float dissolve_volume = 1f;
    [Range(0f, 1f)] public float dissolve_pitch = 1f;

    // ======== TIMING PUZZLE ========
    [Header("Puzzle Timing")]
    [Range(0f, 5f)] public float delayBeforeDissolve = 0.2f; // Ritardo prima che parta la dissolvenza

    // ======== ALTRO ========
    public ChangeCircleMaterial circleMaterialChanger; // Script che cambia i materiali dei cerchi

    // ======== VARIABILI INTERNE ========
    private bool[] circleActivated; // Tiene traccia dei cerchi attivi
    private bool[] circleReserved; // Evita che più particelle vadano sullo stesso cerchio nello stesso momento
    private bool hasCircles; // True se ci sono cerchi
    private bool puzzleSolved = false; // True se tutti i cerchi sono stati attivati

    // ======== INIZIALIZZAZIONE ========
    private void Start()
    {
        hasCircles = circles != null && circles.Length > 0;
        if (hasCircles)
        {
            // Crea array dello stesso size dei cerchi per tenere traccia dello stato
            circleActivated = new bool[circles.Length];
            circleReserved = new bool[circles.Length];
        }

        // Aggiorna subito i colori dei cerchi secondo il mondo attuale
        UpdateCircleColors();

        // Nasconde gli oggetti da mostrare all'inizio
        foreach (GameObject obj in objectsToShow)
            if (obj != null) obj.SetActive(false);
    }

    // ======== PULSANTE PREMUTO ========
    public void ButtonPressed(Transform buttonPos)
    {
        if (puzzleSolved) return; // Se il puzzle è già risolto, non fare nulla

        if (hasCircles)
        {
            // Trova il primo cerchio non attivo e lancia particelle verso di esso
            for (int i = 0; i < circles.Length; i++)
            {
                if (!circleActivated[i] && !circleReserved[i])
                {
                    circleReserved[i] = true; // Prenota il cerchio per evitare conflitti
                    SpawnParticles(buttonPos, circles[i].transform, i);
                    break; // Solo un cerchio alla volta
                }
            }
        }
        else
        {
            // Non ci sono cerchi: attiva/disattiva subito gli oggetti
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

    // ======== CERCHIO ATTIVATO ========
    public void ActivateCircle(int index)
    {
        if (!hasCircles || circleActivated[index]) return;

        circleActivated[index] = true; // Segna il cerchio come attivo
        circleReserved[index] = false; // Libera il cerchio prenotato

        // Particelle burst quando si attiva
        if (circleBurst_Real == null || circleBurst_Fantasy==null)
            Debug.LogWarning("Burst prefab non assegnati! Assegna i prefab di burst per il mondo reale e fantasy.");
        else 
        { 
            AudioManager.Instance.PlaySFX(AudioManager.Instance.glowingSound);

            Vector3 pos = circles[index].transform.position + Random.insideUnitSphere * 0.05f;

            // Scegli il burst corretto a seconda del mondo
            ParticleSystem burstPrefab = WorldSwitch.Instance.isFantasyWorldActive ? circleBurst_Fantasy : circleBurst_Real;

            ParticleSystem burst = Instantiate(burstPrefab, pos, Quaternion.identity);
            burst.Play();
            Destroy(burst.gameObject, 2f); // Distrugge dopo l'animazione
        }

        // Glow interno
        SpriteRenderer glow = circles[index].transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (glow != null)
            glow.color = WorldSwitch.Instance.isFantasyWorldActive ? int_glowColor_Fantasy : int_glowColor_Real;

        // Glow esterno (shader/materiale)
        Renderer circleRenderer = circles[index].GetComponent<Renderer>();
        if (circleRenderer != null)
        {
            Material mat = new Material(circleRenderer.material); // Copia materiale
            mat.SetColor("_HitEffectColor", WorldSwitch.Instance.isFantasyWorldActive ? ext_glowColor_Fantasy : ext_glowColor_Real);
            circleRenderer.material = mat;
        }

        // Attiva animazioni se esiste Animator
        Animator anim = circles[index].GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("Active");

        // Se tutti i cerchi sono attivi, parte la dissolvenza
        if (AllCirclesActive())
        {
            puzzleSolved = true;
            StartCoroutine(HideAndShowObjects());
        }
    }

    // ======== CONTROLLA SE TUTTI I CERCHI SONO ATTIVI ========
    private bool AllCirclesActive()
    {
        if (!hasCircles) return true;

        foreach (bool b in circleActivated)
            if (!b) return false;

        return true;
    }

    // ======== NASCONDI/SHOW OGGETTI CON DISSOLVENZA ========
    private IEnumerator HideAndShowObjects()
    {
        yield return new WaitForSeconds(delayBeforeDissolve); // Attendi un po' prima di far partire la dissolvenza

        // Cambia materiale dei cerchi per permettere la dissolvenza
        if (circleMaterialChanger != null)
            circleMaterialChanger.SwitchMaterial(circles);

        // Dissolvi oggetti da nascondere
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null && obj.activeInHierarchy)
            {
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
        }

        yield return new WaitForSeconds(0.8f); // Attendi che la dissolvenza finisca

        // Mostra oggetti
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

    // ======== LANCIA PARTICELLE VERSO UN CERCHIO ========
    private void SpawnParticles(Transform start, Transform target, int circleIndex)
    {
        if (!hasCircles) return;

        ParticleSystem ps = WorldSwitch.Instance.isFantasyWorldActive
            ? Instantiate(particle_Fantasy, start.position, Quaternion.identity)
            : Instantiate(particle_Real, start.position, Quaternion.identity);

        ParticlesToTarget mover = ps.GetComponent<ParticlesToTarget>();
        if (mover != null)
            mover.Init(target, this, circleIndex, particleSpeed);

        ps.Play();
    }

    // ======== AGGIORNA COLORI DEI CERCHI ========
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
                extColor = Color.black; // Nessun glow esterno per i cerchi spenti
            }

            // Glow interno
            SpriteRenderer glow = circles[i].transform.GetChild(0).GetComponent<SpriteRenderer>();
            if (glow != null)
                glow.color = intColor;

            // Glow esterno shader/materiale
            Renderer circleRenderer = circles[i].GetComponent<Renderer>();
            if (circleRenderer != null)
                circleRenderer.material.SetColor("_HitEffectColor", extColor);
        }
    }
}