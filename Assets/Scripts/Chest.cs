using UnityEngine;
using System.Collections;


public class Chest : MonoBehaviour
{
    private Animator animator;

    public bool needKey = false;
    private bool isOpen = false;


    public GameObject coinPrefab;
    public int coinCount = 6;
    public float spread = 0.5f;           // dispersione orizzontale spawn
    public float minUpForce = 3f;
    public float maxUpForce = 5f;
    public float sideForce = 1.5f;        // forza laterale massima
    public float spawnDelay = 0.06f;      // ritardo fra una moneta e l'altra

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isOpen)
        {
            if (other.CompareTag("Player"))
            {
                PlayerInventory playerInventory = other.GetComponent<PlayerInventory>();
                if (needKey)
                {
                    if (playerInventory != null && playerInventory.HasKey())
                    {
                        OpenChest();
                        playerInventory.UseKey();
                    }
                    else
                    {
                        StartCoroutine(ShakeChest());
                    }
                }
                else
                {
                    OpenChest();
                }
            }
        }
    }


    private void OpenChest()
    {
       isOpen = true;
       animator.SetTrigger("Open");

        // spawn delle monete
        StartCoroutine(SpawnCoins());
    }


    private IEnumerator ShakeChest(float duration = 0.15f, float magnitude = 0.05f)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }


    private IEnumerator SpawnCoins()
    {
        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnPos = transform.position + Vector3.down * 0.1f + new Vector3(Random.Range(-spread, spread), 0f, 0f);
            GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);

            Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // forza casuale verso l'alto e + component laterale casuale
                float up = Random.Range(minUpForce, maxUpForce);
                float sx = Random.Range(-sideForce, sideForce);
                rb.AddForce(new Vector2(sx, up), ForceMode2D.Impulse);
            }
            yield return new WaitForSeconds(spawnDelay);
        }
    }
}
