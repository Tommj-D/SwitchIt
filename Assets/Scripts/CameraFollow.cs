using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // Il giocatore da seguire
    [SerializeField] private float smoothSpeed = 5f; // Velocità di "inseguimento"
    [SerializeField] private Vector3 offset; // Offset della camera rispetto al giocatore

    [Header("Limiti della camera")]
    [SerializeField] private bool useLimits = false;
    [SerializeField] private Vector2 minPosition; // limite inferiore
    [SerializeField] private Vector2 maxPosition; // limite superiore

    private void LateUpdate()
    {
        if (target == null) return;

        // Posizione desiderata
        Vector3 desiredPosition = target.position + offset;

        // Applica i limiti se attivi
        if (useLimits)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minPosition.x, maxPosition.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minPosition.y, maxPosition.y);
        }

        // Movimento fluido verso il target
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }

    private void OnDrawGizmosSelected()
{
    if (useLimits)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(minPosition.x, minPosition.y, 0), new Vector3(maxPosition.x, minPosition.y, 0));
        Gizmos.DrawLine(new Vector3(minPosition.x, maxPosition.y, 0), new Vector3(maxPosition.x, maxPosition.y, 0));
        Gizmos.DrawLine(new Vector3(minPosition.x, minPosition.y, 0), new Vector3(minPosition.x, maxPosition.y, 0));
        Gizmos.DrawLine(new Vector3(maxPosition.x, minPosition.y, 0), new Vector3(maxPosition.x, maxPosition.y, 0));
    }
}

}
