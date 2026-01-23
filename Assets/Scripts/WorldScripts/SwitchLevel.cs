using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class LevelEndPortal : MonoBehaviour
{
    public string nextSceneName;

    public float teleportDelay = 0.5f;
    public GameObject teleportEffect;

    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            activated = true;
            StartCoroutine(LevelCompleteSequence(other.gameObject));
        }
    }

    private IEnumerator LevelCompleteSequence(GameObject player)
    {
        // Blocca input e movimento
        PlayerInput input = player.GetComponent<PlayerInput>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (input != null) input.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // FX magico
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, player.transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(teleportDelay);

        SceneController.Instance.LoadScene(nextSceneName);
    }
}
