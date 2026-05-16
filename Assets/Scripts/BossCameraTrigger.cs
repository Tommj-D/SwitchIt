using UnityEngine;

public class BossCameraTrigger : MonoBehaviour
{
    [Header("Impostazioni Inquadratura")]
    public Transform puntoCentrale;

    [Tooltip("Quanto deve indietreggiare la camera?")]
    public float distanzaZ = -25f;

    public float velocitaTransizione = 3f;

    [Header("Blocco Arena")]
    public GameObject muroDietro;

    private Camera cam;
    private Behaviour cinemachineBrain;

    private bool isBossFightActive = false;

    private void Start()
    {
        cam = Camera.main;

        if (cam != null)
        {
            cinemachineBrain = cam.GetComponent("CinemachineBrain") as Behaviour;
        }

        // Assicura che il muro sia spento all'inizio
        if (muroDietro != null)
            muroDietro.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isBossFightActive)
        {
            isBossFightActive = true;

            // Attiva il muro
            if (muroDietro != null)
                muroDietro.SetActive(true);

            // Spegne Cinemachine
            if (cinemachineBrain != null)
                cinemachineBrain.enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (isBossFightActive && cam != null)
        {
            Vector3 targetPos = new Vector3(
                puntoCentrale.position.x,
                puntoCentrale.position.y,
                distanzaZ
            );

            cam.transform.position = Vector3.Lerp(
                cam.transform.position,
                targetPos,
                Time.deltaTime * velocitaTransizione
            );
        }
    }

    public void ResetCamera()
    {
        isBossFightActive = false;

        // Spegne il muro quando muori/resetti
        if (muroDietro != null)
            muroDietro.SetActive(false);

        if (cinemachineBrain != null)
            cinemachineBrain.enabled = true;
    }
}