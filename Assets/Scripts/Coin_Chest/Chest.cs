using UnityEngine;
using System.Collections;

public class Chest : MonoBehaviour
{
    private Animator animator;

    public AudioClip collectSound;
    public GameObject collectEffect;

    public bool needKey = false;
    private bool isOpen = false;

    public GameObject dropPrefab;        // prefab dell’oggetto che esce (moneta/chiave/pozione)
    public int amount = 1;               // quantità (es: monete = 1)

    public float finalPosition = 1.5f;   // altezza finale dell’oggetto che esce
    public float duration = 0.4f;              // durata dell’animazione di uscita
    public float waitingTime = 0.05f;     // tempo di attesa tra un’uscita e l’altra

    public bool stayVisible = false;       // se true, l'oggetto rimane visibile per un po' prima di scomparire
    public float visibleDuration = 1.5f; // tempo prima che scompaia 
    public float finalFadeDuration = 0.5f; // quanto dura il fade-out

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

        for (int i = 0; i < amount; i++)
        {
            // crea l’oggetto
            GameObject drop = Instantiate(dropPrefab, transform.position, Quaternion.identity);

            Vector3 start = transform.position;
            Vector3 end = start + Vector3.up * finalPosition;

            float elapsed = 0f;

            // sprite fade
            SpriteRenderer sr = drop.GetComponent<SpriteRenderer>();
            Color originalColor = sr != null ? sr.color : Color.white;

            while (elapsed < duration)
            {
                float t = elapsed / duration;

                drop.transform.position = Vector3.Lerp(start, end, t);
                drop.transform.localScale = Vector3.one * Mathf.Lerp(0.4f, 1.2f, t);

                // Se stayVisible==false facciamo il fade DURANTE la salita
                if (sr != null)
                {
                    if (!stayVisible)
                        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - t);
                    else
                        sr.color = originalColor; // mantieni visibile durante la salita
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Assicuriamo pos e scala finali
            drop.transform.position = end;
            drop.transform.localScale = Vector3.one * (stayVisible ? 1f : 1.2f);

            if (sr != null && !stayVisible)
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // garantito invisible

            // SE stayVisible: aspetta visibleDuration prima di iniziare il fade finale
            if (stayVisible)
            {
                float stayElapsed = 0f;
                while (stayElapsed < visibleDuration)
                {
                    // forziamo la posizione ad ogni frame per evitare spostamenti indesiderati
                    drop.transform.position = end;

                    // mantieni visibilità piena mentre resta sospesa
                    if (sr != null)
                        sr.color = originalColor;

                    stayElapsed += Time.deltaTime;
                    yield return null;
                }

                // 3) fade-out finale morbido (resta in aria mentre svanisce)
                float fadeElapsed = 0f;
                while (fadeElapsed < finalFadeDuration)
                {
                    float tf = fadeElapsed / finalFadeDuration;
                    if (sr != null)
                    {
                        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - tf);
                    }

                    // manteniamo la posizione mentre svanisce
                    drop.transform.position = end;

                    fadeElapsed += Time.deltaTime;
                    yield return null;
                }
            }

            Destroy(drop);

            // Effetto visivo nella posizione finale della moneta
            if (collectEffect != null)
                Instantiate(collectEffect, drop.transform.position, Quaternion.identity);

            // Suono nella posizione finale della moneta
            if (collectSound != null)
                AudioSource.PlayClipAtPoint(collectSound, drop.transform.position);

            // ---- AGGIUNTA AL PLAYER ----
            if (inv != null)
                inv.AddCoins(1);

            // attesa prima della prossima moneta
            yield return new WaitForSeconds(waitingTime);
        }
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
