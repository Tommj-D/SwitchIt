using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;                 // valore della moneta
    public GameObject collectEffect;       // effetto quando raccolta
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
        // Animazione di galleggiamento
        float y = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPos.x, startPos.y + y, startPos.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !collected)
        {
            Collect(); 
        }
    }

    public void Collect()
    {

        // SUONO 
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.coinSound);

        collected = true;

        // Disabilita il collider così non viene raccolta due volte
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // PUNTEGGIO
        if (GameManager.Instance != null)
            GameManager.Instance.AddCoin(value);

        // EFFETTO VISIVO
        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        // DISTRUZIONE
        Destroy(gameObject, destroyDelay);
    }
}