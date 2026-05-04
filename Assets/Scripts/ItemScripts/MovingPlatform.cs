using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Impostazioni Percorso")]
    public Transform posA;
    public Transform posB;
    public float speed = 3f;

    [Header("Effetti Visivi")]
    public string hexFantasyColor = "#1E90FF"; // Il colore azzurro magico!
    private Color fantasyColor;
    private Color normalColor;
    private SpriteRenderer sr;

    private Vector3 targetPos;
    private Rigidbody2D rb;
    private List<Rigidbody2D> passeggeri = new List<Rigidbody2D>();

    void Start()
    {
        targetPos = posB.position;
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // --- PREPARAZIONE COLORI ---
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            normalColor = sr.color; // Salva il colore originale per quando torni al mondo normale
        }
        
        // Converte il codice esadecimale in un colore che Unity può usare
        ColorUtility.TryParseHtmlString(hexFantasyColor, out fantasyColor);
    }

    // Usiamo Update per gli effetti visivi (è più fluido del FixedUpdate)
    void Update()
    {
        if (sr != null && WorldSwitch.Instance != null)
        {
            if (WorldSwitch.Instance.isFantasyWorldActive)
            {
                sr.color = fantasyColor; // Diventa azzurra
            }
            else
            {
                sr.color = normalColor; // Torna normale
            }
        }
    }

    void FixedUpdate()
    {
        // --- LA MAGIA DEL BLOCCO DIMENSIONALE ---
        if (WorldSwitch.Instance != null && WorldSwitch.Instance.isFantasyWorldActive)
        {
            return; // Ferma tutto il movimento fisico
        }

        // --- MOVIMENTO NORMALE ---
        Vector2 nuovaPosizione = Vector2.MoveTowards(rb.position, targetPos, speed * Time.fixedDeltaTime);
        Vector2 spostamento = nuovaPosizione - rb.position;
        rb.MovePosition(nuovaPosizione);

        foreach (Rigidbody2D passeggero in passeggeri)
        {
            if (passeggero != null)
            {
                passeggero.position += spostamento;
            }
        }

        if (Vector2.Distance(rb.position, targetPos) < 0.05f)
        {
            if (targetPos == posA.position)
                targetPos = posB.position;
            else
                targetPos = posA.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
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
}