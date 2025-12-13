using UnityEngine;

public class Coin : MonoBehaviour, IItem
{
    [SerializeField] private GameObject collectEffect;       // effetto quando raccolta
    [SerializeField] private AudioClip collectSound;         // suono quando raccolta
    [SerializeField] private float destroyDelay = 0.1f;      // ritardo distruzione
    private bool collected = false;                          // per evitare doppie raccolte

    [SerializeField] private float amplitude = 0.15f;   // quanto sale/scende
    [SerializeField] private float frequency = 2f;      // velocità dell'oscillazione

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected) return;

        if (collision.CompareTag("Player"))
        {
            collected = true;

            // Effetto visivo
            if (collectEffect != null)
                Instantiate(collectEffect, transform.position, Quaternion.identity);

            // Suono
            if (collectSound != null)
                AudioSource.PlayClipAtPoint(collectSound, transform.position);

            // Disattiva grafica e collider
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;

            // Distrugge l’oggetto dopo breve delay
            Destroy(gameObject, destroyDelay);
        }
    }
    public void Collect()
    {
        throw new System.NotImplementedException();
    }
}
