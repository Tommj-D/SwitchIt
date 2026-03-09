using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Button : MonoBehaviour
{
    private bool activated = false;
    private Animator animator;

    [Header("Effetti")]
    public ParticleSystem particellePolvere; 
    [Header("Impostazioni Dissolvenza")]
    public float durataDissolvenza = 1.0f;

    [Header("Oggetti da NASCONDERE")]
    public GameObject[] oggettiDaNascondere;

    [Header("Oggetti da MOSTRARE")]
    public GameObject[] oggettiDaMostrare;

    public bool isAnimated = true;

    private void Start()
    {
        if (isAnimated) 
            animator = GetComponentInChildren<Animator>();

        foreach (GameObject obj in oggettiDaMostrare)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        activated = true;

        if (particellePolvere != null)
        {
            particellePolvere.Play();
        }

        if (animator != null && isAnimated)
        {
            animator.SetTrigger("Press");
            
            if (AudioManager.Instance != null && AudioManager.Instance.sfxSource != null)
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buttonSound);
        }
        else
        {
            AttivaOggetti();
        }
    }

    public void AttivaOggetti()
    {
        foreach (GameObject obj in oggettiDaNascondere)
        {
            if (obj != null) 
            {
                StartCoroutine(DissolviENascondi(obj));
            }
        }

        foreach (GameObject obj in oggettiDaMostrare)
        {
            if (obj != null) obj.SetActive(true);
        }
    }

    private IEnumerator DissolviENascondi(GameObject obj)
    {
        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        Tilemap tilemap = obj.GetComponent<Tilemap>();
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();

        float timer = 0f;

        if (tilemap != null)
        {
            Color colore = tilemap.color;
            float alphaIniziale = colore.a;

            while (timer < durataDissolvenza)
            {
                timer += Time.deltaTime;
                colore.a = Mathf.Lerp(alphaIniziale, 0f, timer / durataDissolvenza);
                tilemap.color = colore;
                yield return null;
            }
            colore.a = 0f;
            tilemap.color = colore;
        }
        else if (spriteRenderer != null)
        {
            Color colore = spriteRenderer.color;
            float alphaIniziale = colore.a;

            while (timer < durataDissolvenza)
            {
                timer += Time.deltaTime;
                colore.a = Mathf.Lerp(alphaIniziale, 0f, timer / durataDissolvenza);
                spriteRenderer.color = colore;
                yield return null;
            }
            colore.a = 0f;
            spriteRenderer.color = colore;
        }
        else
        {
            yield return new WaitForSeconds(durataDissolvenza);
        }

        obj.SetActive(false);
    }
}