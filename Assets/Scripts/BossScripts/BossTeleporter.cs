using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class BossTeleporter : MonoBehaviour
{
    [Header("Destinazione")]
    [Tooltip("L'oggetto vuoto nel nuovo corridoio dove deve apparire il player")]
    public Transform puntoDiArrivo;

    [Header("Nuova Telecamera (Opzionale)")]
    [Tooltip("Trascina qui l'oggetto Virtual Camera della stanza del boss da accendere")]
    public GameObject virtualCameraBoss;
    [Tooltip("Trascina qui l'oggetto Virtual Camera del livello da spegnere")]
    public GameObject virtualCameraLivello;

    [Header("Effetto Uscita (Camminata Automatica)")]
    public float tempoCamminataForzata = 0.4f;
    public float velocitaUscita = 8f;
    public float direzioneUscita = 1f;

    private bool isTeleporting = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isTeleporting)
        {
            StartCoroutine(TeleportSequence(collision.gameObject));
        }
    }

    private IEnumerator TeleportSequence(GameObject player)
    {
        isTeleporting = true;

        // 1. BLOCCA I CONTROLLI
        PlayerInput input = player.GetComponent<PlayerInput>();
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (input != null) input.enabled = false;
        if (movement != null) movement.enabled = false; 
        if (rb != null) rb.linearVelocity = Vector2.zero; 

        // 2. SCHERMO NERO
        if (SceneController.Instance != null)
            yield return StartCoroutine(SceneController.Instance.FadeOut(SceneController.Instance.fadeDuration));

        // 3. TELETRASPORTO
        player.transform.position = puntoDiArrivo.position;

        // 4. CAMBIO TELECAMERA (Se le hai assegnate)
        if (virtualCameraBoss != null) virtualCameraBoss.SetActive(true);
        if (virtualCameraLivello != null) virtualCameraLivello.SetActive(false);

        // Snap rapido della camera principale per evitare sfarfallii
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(puntoDiArrivo.position.x, puntoDiArrivo.position.y, mainCam.transform.position.z);
        }

        // 5. RIAPRE LO SCHERMO
        if (SceneController.Instance != null)
            StartCoroutine(SceneController.Instance.FadeIn(SceneController.Instance.fadeDuration));

        // 6. CAMMINATA AUTOMATICA
        float timer = 0f;
        while (timer < tempoCamminataForzata)
        {
            timer += Time.deltaTime;
            if (rb != null) rb.linearVelocity = new Vector2(velocitaUscita * direzioneUscita, rb.linearVelocity.y);
            yield return null;
        }

        // 7. RIDÀ I CONTROLLI
        if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); 
        if (movement != null) movement.enabled = true;
        if (input != null) input.enabled = true;

        isTeleporting = false;
    }
}