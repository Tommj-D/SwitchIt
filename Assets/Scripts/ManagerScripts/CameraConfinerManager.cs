using UnityEngine;
using Unity.Cinemachine;

public class CameraConfinerManager : MonoBehaviour
{
    private CinemachineConfiner2D confiner;

    private void Awake()
    {
        confiner = GetComponent<CinemachineConfiner2D>();
    }

    public void SetConfiner(Collider2D newConfiner)
    {
        if (confiner == null || newConfiner == null) return;

        confiner.BoundingShape2D = newConfiner;
        confiner.InvalidateBoundingShapeCache();
    }
}
