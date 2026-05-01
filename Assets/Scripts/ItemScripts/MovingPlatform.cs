using System.Collections.Generic; // Ci serve per fare la lista dei "passeggeri"
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Impostazioni Percorso")]
    public Transform posA;
    public Transform posB;
    public float speed = 3f;

    private Vector3 targetPos;
    private Rigidbody2D rb;
    
    private List<Rigidbody2D> passeggeri = new List<Rigidbody2D>();

    void Start()
    {
        targetPos = posB.position;
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void FixedUpdate()
    {
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