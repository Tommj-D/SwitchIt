using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float fixedZ = 0f; // oppure quello che vuoi

    void LateUpdate()
    {
        transform.position = new Vector3(
            player.position.x,
            player.position.y,
            fixedZ
        );
    }
}