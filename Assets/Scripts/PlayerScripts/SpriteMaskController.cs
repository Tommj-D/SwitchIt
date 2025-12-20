using UnityEngine;

public class SpriteMaskController : MonoBehaviour
{
    public UnityEngine.SpriteMask spriteMask;

    [Header("Flip Movement")]
    public float flipSpeed = 15f;

    private Vector3 startLocalPos;
    private Vector3 targetLocalPos;

    private void Start()
    {
        if (spriteMask != null)
            spriteMask.enabled = false; // disabilita la maschera all'avvio

        startLocalPos = spriteMask.transform.localPosition;
        targetLocalPos = startLocalPos;
    }

    private void Update()
    {
        if (spriteMask == null) return;

        spriteMask.transform.localPosition = Vector3.Lerp(
            spriteMask.transform.localPosition,
            targetLocalPos,
            Time.deltaTime * flipSpeed
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger OK");
        if (other.CompareTag("Hidden") && spriteMask != null)
            spriteMask.enabled = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Hidden") && spriteMask != null)
            spriteMask.enabled = false;
    }


    public void FaceRight()
    {
        if (spriteMask == null) return;

        targetLocalPos = new Vector3(
            Mathf.Abs(startLocalPos.x),
            startLocalPos.y,
            startLocalPos.z
        );
    }

    public void FaceLeft()
    {
        if (spriteMask == null) return;

        targetLocalPos = new Vector3(
            -Mathf.Abs(startLocalPos.x),
            startLocalPos.y,
            startLocalPos.z
        );
    }
}
