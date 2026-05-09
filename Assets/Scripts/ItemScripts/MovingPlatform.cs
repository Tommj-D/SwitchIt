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

        // Imposta subito il colore corretto all'avvio
        UpdateColor();
    }

    void Update()
    {
        UpdateColor();
    }

    void UpdateColor()
    {
        if (spriteRenderers == null || WorldSwitch.Instance == null)
            return;

        Color targetColor;

        if (WorldSwitch.Instance.isFantasyWorldActive)
        {
            targetColor = fantasyColor;
        }
        else
        {
            targetColor = normalColor;
        }

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.color = targetColor;
            }
        }
    }

    void FixedUpdate()
    {
        if (WorldSwitch.Instance != null)
        {
            bool isFantasy = WorldSwitch.Instance.isFantasyWorldActive;

            if (isFantasy && !moveFantasy)
                return;

            if (!isFantasy && !moveReal)
                return;
        }

        // Movimento piattaforma
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

        // Cambio destinazione
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