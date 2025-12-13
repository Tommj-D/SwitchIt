using System;
using UnityEngine;

public class Coin : MonoBehaviour, IItem
{
    public int value = 1;                 // valore della moneta

    public GameObject collectEffect;       // effetto quando raccolta
    public AudioClip collectSound;         // suono quando raccolta
    private float destroyDelay = 0.1f;      // ritardo distruzione

    [Header("Animazione")]
    public float amplitude = 0.15f;   // quanto sale/scende
    public float frequency = 2f;      // velocità dell'oscillazione

    private bool collected = false;    // stato raccolta

    private Vector3 startPos;
    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPos.x, startPos.y + y, startPos.z);
    }

    public void Collect()
    {
        if (collected) return;

        collected = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Notifica il sistema di punteggio
        GameManager.instance.AddCoin(value);

        // Effetto visivo
        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        // Suono
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        // Distrugge l’oggetto dopo breve delay
        Destroy(gameObject, destroyDelay);
    }
}
