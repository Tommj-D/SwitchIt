using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;

public class MagicTeleport : MonoBehaviour
{
    [Header("Layer Settings")]
    public string nuovoLayer = "Player_Back";

    [Header("Sorting Layer")]
    public string nuovoSortingLayer = "PlayerBack";
    public int nuovoOrderInLayer = 5;

    [Header("Impostazioni Teletrasporto")]
    public Transform destinazione;
    public Image schermoNero;
    public float durataFade = 0.5f;

    [Header("Impostazioni Fisiche")]
    public float forzaSaltoUscita = 15f;

    [Header("Camera Confiner")]
    public CameraConfinerManager cameraConfinerManager;
    public Collider2D confinerDestinazione;

    [Header("Effetti Sonori (Opzionali)")]
    public AudioClip suonoEntrata;
    public AudioClip suonoUscita;

    private bool inCorso = false;
    private Vector3 scalaOriginalePlayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !inCorso)
        {
            StartCoroutine(SequenzaCadutaSalto(collision.gameObject));
        }
    }

    private IEnumerator SequenzaCadutaSalto(GameObject player)
    {
        inCorso = true;
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        scalaOriginalePlayer = player.transform.localScale;

        // --- SUONO ENTRATA ---
        if (suonoEntrata != null)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(suonoEntrata);
            else
                AudioSource.PlayClipAtPoint(suonoEntrata, transform.position);
        }

        // --- FASE 1: FADE OUT + SHRINK ---
        float timer = 0;
        while (timer < durataFade)
        {
            timer += Time.deltaTime;
            float t = timer / durataFade;

            if (schermoNero != null)
            {
                Color c = schermoNero.color;
                c.a = Mathf.Lerp(0, 1, t);
                schermoNero.color = c;
            }

            player.transform.localScale = Vector3.Lerp(scalaOriginalePlayer, Vector3.zero, t);

            yield return null;
        }

        if (schermoNero != null)
        {
            Color c = schermoNero.color;
            c.a = 1;
            schermoNero.color = c;
        }

        player.transform.localScale = Vector3.zero;

        // --- TELETRASPORTO ---
        player.transform.position = destinazione.position;

        // 🔥 APPLICA TUTTO (layer + sorting + particelle)
        ApplyLayerAndSorting(player);

        player.transform.localScale = scalaOriginalePlayer;

        cameraConfinerManager.SetConfiner(confinerDestinazione);

        yield return new WaitForSeconds(0.1f);

        // 🔥 RIAPPLICA DOPO UN FRAME (fix per ragdoll / oggetti attivati dopo)
        yield return null;
        ApplyLayerAndSorting(player);

        // --- SUONO USCITA ---
        if (suonoUscita != null)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(suonoUscita);
            else
                AudioSource.PlayClipAtPoint(suonoUscita, destinazione.position);
        }

        // --- SALTO ---
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * forzaSaltoUscita, ForceMode2D.Impulse);
        }

        // --- FADE IN ---
        timer = 0;
        while (timer < durataFade)
        {
            timer += Time.deltaTime;
            float t = timer / durataFade;

            if (schermoNero != null)
            {
                Color c = schermoNero.color;
                c.a = Mathf.Lerp(1, 0, t);
                schermoNero.color = c;
            }

            yield return null;
        }

        if (schermoNero != null)
        {
            Color c = schermoNero.color;
            c.a = 0;
            schermoNero.color = c;
        }

        inCorso = false;
    }

    // METODO UNICO COMPLETO
    void ApplyLayerAndSorting(GameObject obj)
    {
        int layer = LayerMask.NameToLayer(nuovoLayer);

        // Layer ricorsivo
        SetLayerRecursively(obj, layer);

        // Sprite Renderer
        SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sr in renderers)
        {
            sr.sortingLayerName = nuovoSortingLayer;
            sr.sortingOrder = nuovoOrderInLayer;
        }

        // Particle System
        ParticleSystemRenderer[] particles = obj.GetComponentsInChildren<ParticleSystemRenderer>(true);
        foreach (var p in particles)
        {
            p.sortingLayerName = nuovoSortingLayer;
            p.sortingOrder = nuovoOrderInLayer;
        }

        // Sorting Group 
        SortingGroup[] groups = obj.GetComponentsInChildren<SortingGroup>(true);
        foreach (SortingGroup sg in groups)
        {
            sg.sortingLayerName = nuovoSortingLayer;
            sg.sortingOrder = nuovoOrderInLayer;
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}