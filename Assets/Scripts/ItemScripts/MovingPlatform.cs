using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Impostazioni Percorso")]
    public Transform posA;
    public Transform posB;
    public float speed = 3f;

    [Header("Condizioni di Partenza")]
    [Tooltip("Se attivato, la piattaforma resterà ferma finché il player non ci salta sopra.")]
    public bool moveOnlyWhenSteppedOn = false;

    [Header("Colori")]
    public Color normalColor = Color.white;
    public Color fantasyColor = Color.cyan;

    [Header("Movimento Dimensioni")]
    public bool moveReal = true;
    public bool moveFantasy = true;

    [Header("Sicurezza Salto")]
    [Tooltip("Deve essere più basso della tua forza di salto, ma più alto della velocità della piattaforma.")]
    public float limiteInerzia = 8f; 

    private SpriteRenderer[] spriteRenderers;
    private Vector3 targetPos;
    private Rigidbody2D rb;
    private List<Rigidbody2D> passeggeri = new List<Rigidbody2D>();
    
    // Variabile interna per sapere se il player l'ha già "svegliata"
    private bool hasBeenSteppedOn = false;

    void Start()
    {
        targetPos = posB.position;
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        UpdateColor();
    }

    void Update() { UpdateColor(); }

    void UpdateColor()
    {
        if (spriteRenderers == null || WorldSwitch.Instance == null) return;

        Color targetColor = WorldSwitch.Instance.isFantasyWorldActive ? fantasyColor : normalColor;
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null) sr.color = targetColor;
        }
    }

    void FixedUpdate()
    {
        // --- NUOVO CONTROLLO: Deve aspettare il player? ---
        if (moveOnlyWhenSteppedOn && !hasBeenSteppedOn)
        {
            return; // Interrompe qui e non muove nulla
        }

        if (WorldSwitch.Instance != null)
        {
            bool isFantasy = WorldSwitch.Instance.isFantasyWorldActive;
            if (isFantasy && !moveFantasy) return;
            if (!isFantasy && !moveReal) return;
        }

        Vector2 nuovaPosizione = Vector2.MoveTowards(rb.position, targetPos, speed * Time.fixedDeltaTime);
        Vector2 spostamento = nuovaPosizione - rb.position;
        
        rb.MovePosition(nuovaPosizione);

        // Dentro MovingPlatform.cs, modifica il ciclo for nel FixedUpdate:

    // IL SISTEMA PASSEGGERI UNIVERSALE
        for (int i = passeggeri.Count - 1; i >= 0; i--)
        {
            Rigidbody2D passeggero = passeggeri[i];
            
            if (passeggero == null) 
            {
                passeggeri.RemoveAt(i);
                continue;
            }

            // Spostamento Universale per tutti
            passeggero.position += spostamento;

            // LA GHIGLIOTTINA DELL'INERZIA: ORA AGISCE SOLO SUL PLAYER!
            // Ignoriamo gli slime, così Unity è libero di calcolare la loro collisione in pace
            if (passeggero.CompareTag("Player"))
            {
                if (passeggero.linearVelocity.y > 0 && passeggero.linearVelocity.y < limiteInerzia)
                {
                    passeggero.linearVelocity = new Vector2(passeggero.linearVelocity.x, -2f);
                }
            }
        }

        if (Vector2.Distance(rb.position, targetPos) < 0.05f)
        {
            targetPos = (targetPos == posA.position) ? posB.position : posA.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Sblocca la piattaforma non appena il player la tocca
            hasBeenSteppedOn = true;

            Rigidbody2D rbGiocatore = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rbGiocatore != null && !passeggeri.Contains(rbGiocatore))
            {
                passeggeri.Add(rbGiocatore);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rbGiocatore = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rbGiocatore != null && passeggeri.Contains(rbGiocatore))
            {
                passeggeri.Remove(rbGiocatore);
            }
        }
    }

    public void AggiungiPasseggero(Rigidbody2D passeggeroExtra)
    {
        if (passeggeroExtra != null && !passeggeri.Contains(passeggeroExtra))
        {
            passeggeri.Add(passeggeroExtra);
        }
    }
}