using UnityEngine;
using UnityEngine.InputSystem;

public class MenuParallax : MonoBehaviour
{
    [Header("Impostazioni Parallasse")]
    [Tooltip("Quanto si muove l'oggetto. Usa valori separati per X e Y.")]
    public Vector2 offsetMultiplier = new Vector2(0.5f, 0.5f);
    
    [Tooltip("Quanto è morbido il movimento. Valori più bassi = più reattivo.")]
    public float smoothTime = 0.2f;
    
    [Tooltip("Inverte la direzione del movimento per dare un senso di profondità.")]
    public bool invertDirection = false;

    private Vector3 startPosition;
    private Vector3 velocity;

    private void Start()
    {
        // Salviamo la posizione iniziale al centro
        startPosition = transform.position;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        // 1. Leggiamo la posizione del mouse
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // 2. Normalizziamo la posizione da -1 a 1 (il centro dello schermo diventa 0,0)
        float normalizedX = (mousePos.x / Screen.width) * 2f - 1f;
        float normalizedY = (mousePos.y / Screen.height) * 2f - 1f;
        Vector2 normalizedMousePos = new Vector2(normalizedX, normalizedY);

        // Limitiamo i valori tra -1 e 1 per evitare sbalzi se il mouse esce dalla finestra
        normalizedMousePos.x = Mathf.Clamp(normalizedMousePos.x, -1f, 1f);
        normalizedMousePos.y = Mathf.Clamp(normalizedMousePos.y, -1f, 1f);

        // 3. Invertiamo la direzione se richiesto
        if (invertDirection)
        {
            normalizedMousePos = -normalizedMousePos;
        }

        // 4. Calcoliamo dove deve andare l'oggetto
        Vector3 targetPosition = startPosition + new Vector3(
            normalizedMousePos.x * offsetMultiplier.x,
            normalizedMousePos.y * offsetMultiplier.y,
            0f
        );

        // 5. Muoviamo l'oggetto in modo fluido verso il target
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}