using UnityEngine;

public class BossCameraTrigger : MonoBehaviour
{
    [Header("Impostazioni Inquadratura")]
    public Transform puntoCentrale; 
    
    [Tooltip("Quanto deve indietreggiare la camera? Valori più negativi (es. -20, -30) inquadrano più spazio.")]
    public float distanzaZ = -25f; 
    
    public float velocitaTransizione = 3f; 

    private Camera cam;
    private Behaviour cinemachineBrain;
    
    private Vector3 posizioneOriginale;
    private bool isBossFightActive = false;

    private void Start()
    {
        cam = Camera.main;
        if (cam != null) 
        {
            // Troviamo Cinemachine
            cinemachineBrain = cam.GetComponent("CinemachineBrain") as Behaviour;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isBossFightActive)
        {
            isBossFightActive = true;
            
            // Spegne Cinemachine
            if (cinemachineBrain != null) cinemachineBrain.enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (isBossFightActive && cam != null)
        {
            // La telecamera si sposta sul centro X e Y, ma arretra sulla Z!
            Vector3 targetPos = new Vector3(puntoCentrale.position.x, puntoCentrale.position.y, distanzaZ);
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Time.deltaTime * velocitaTransizione);
        }
    }

    // ==========================================
    // Viene chiamato dal tuo PlayerRespawn quando muori
    // ==========================================
    public void ResetCamera()
    {
        isBossFightActive = false;
        
        // Riaccende Cinemachine, che riporterà la camera sul player alla Z normale
        if (cinemachineBrain != null) cinemachineBrain.enabled = true;
    }
}