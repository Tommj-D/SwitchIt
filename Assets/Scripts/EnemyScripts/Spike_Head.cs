using UnityEngine;

public class Spike_Head : MonoBehaviour
{
    public GameObject impactParticles;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Se tocca un oggetto con layer "Ground"
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.CompareTag("Player"))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.spikeCrashSound);
            }
            // Istanzia le particelle
            if (impactParticles != null)
            {
                Instantiate(impactParticles, transform.position, Quaternion.identity);
            }

            // Distruggi la spina
            gameObject.SetActive(false);
        }
    }
}
