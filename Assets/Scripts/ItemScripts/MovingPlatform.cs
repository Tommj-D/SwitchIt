using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Impostazioni Percorso")]
    public Transform posA;
    public Transform posB;
    public float speed = 3f;

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
        if (WorldSwitch.Instance != null)
        {
            bool isFantasy = WorldSwitch.Instance.isFantasyWorldActive;
            if (isFantasy && !moveFantasy) return;
            if (!isFantasy && !moveReal) return;
        }

        Vector2 nuovaPosizione = Vector2.MoveTowards(rb.position, targetPos, speed * Time.fixedDeltaTime);
        Vector2 spostamento = nuovaPosizione - rb.position;
        
        rb.MovePosition(nuovaPosizione);

        // IL NUOVO SISTEMA BLINDA-PASSEGGERI
        for (int i = passeggeri.Count - 1; i >= 0; i--)
        {
            Rigidbody2D passeggero = passeggeri[i];
            
            if (passeggero == null) 
            {
                passeggeri.RemoveAt(i);
                continue;
            }

            // 1. Lo spostiamo orizzontalmente in modo perfetto
            passeggero.position += new Vector2(spostamento.x, 0);

            // 2. Se l'ascensore scende, lo tiriamo giù fisicamente
            if (spostamento.y < 0)
            {
                passeggero.position += new Vector2(0, spostamento.y);
            }

            // 3. LA GHIGLIOTTINA DELL'INERZIA (Anti-Saltino)
            // Se la fisica sta spingendo il giocatore in su (velocità > 0)...
            // MA questa velocità è inferiore alla forza di un VERO salto (es. minore di 8)...
            if (passeggero.linearVelocity.y > 0 && passeggero.linearVelocity.y < limiteInerzia)
            {
                // Schiacciamo brutalmente la velocità verso il basso!
                // Un valore di -1f lo terrà magicamente e costantemente incollato al pavimento,
                // eliminando qualsiasi accumulo di inerzia quando arriva in cima.
                passeggero.linearVelocity = new Vector2(passeggero.linearVelocity.x, -1f);
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