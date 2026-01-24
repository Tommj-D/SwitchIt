using UnityEngine;

public class TeleportPullFX : MonoBehaviour
{
    public Transform target;
    public float pullSpeed = 8f;

    void Update()
    {
        if (!target) return;

        transform.position = Vector3.Lerp(
            transform.position,
            target.position,
            pullSpeed * Time.deltaTime
        );
    }
}
