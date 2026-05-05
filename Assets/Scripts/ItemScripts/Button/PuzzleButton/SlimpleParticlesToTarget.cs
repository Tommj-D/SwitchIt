using UnityEngine;

public class SimpleParticleToTarget : MonoBehaviour
{
    public float speed = 5f;
    private Vector3 target;
    private System.Action onArrive;

    public void Init(Vector3 targetPos, System.Action callback)
    {
        target = targetPos;
        onArrive = callback;
    }

    void Update()
    {   
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            onArrive?.Invoke();
            Destroy(gameObject);
        }
    }
}