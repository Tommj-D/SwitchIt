using UnityEngine;
using System.Collections;

public class Chest : MonoBehaviour
{
    private Animator animator;

    public bool needKey = false;
    private bool isOpen = false;

    [Header("Drop Settings")]
    public GameObject dropPrefab;        // prefab dell’oggetto che esce (moneta/chiave/pozione)
    public int amount = 1;               // quantità (es: monete = 1)

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isOpen && other.CompareTag("Player"))
        {
            PlayerInventory inv = other.GetComponent<PlayerInventory>();

            if (needKey)
            {
                if (inv != null && inv.HasKey())
                {
                    OpenChest(inv);
                    inv.UseKey();
                }
                else
                {
                    StartCoroutine(ShakeChest());
                }
            }
            else
            {
                OpenChest(inv);
            }
        }
    }

    private void OpenChest(PlayerInventory inv)
    {
        isOpen = true;
        animator.SetTrigger("Open");
        StartCoroutine(SpawnDrop(inv));
    }

    private IEnumerator SpawnDrop(PlayerInventory inv)
    {
        if (dropPrefab == null)
            yield break;

        // crea l’oggetto all’uscita della chest
        GameObject drop = Instantiate(dropPrefab, transform.position, Quaternion.identity);

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.up * 1f; // altezza a cui arriva

        float duration = 0.5f;
        float elapsed = 0f;

        // sprite per il fade
        SpriteRenderer sr = drop.GetComponent<SpriteRenderer>();
        Color originalColor = sr.color;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            drop.transform.position = Vector3.Lerp(start, end, t);
            drop.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.2f, t);

            if (sr != null)
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // finito: aggiunge all’inventario
        if (inv != null)
            inv.AddCoins(amount);

        Destroy(drop);
    }

    private IEnumerator ShakeChest(float duration = 0.15f, float magnitude = 0.05f)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}



/*using UnityEngine;
using System.Collections;


public class Chest : MonoBehaviour
{
    private Animator animator;

    public bool needKey = false;
    private bool isOpen = false;

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
}*/
