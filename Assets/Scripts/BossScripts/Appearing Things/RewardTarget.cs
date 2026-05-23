using UnityEngine;

public class RewardTarget : MonoBehaviour
{
    [Header("Visuale Da Mostrare")]
    [SerializeField] private GameObject visualObject;

    [Header("Effetto Burst")]
    [SerializeField] private ParticleSystem revealBurst;

    private bool revealed = false;

    private void Start()
    {
        if (visualObject != null)
        {
            visualObject.SetActive(false);
        }
    }

    public void Reveal()
    {
        if (revealed) return;

        revealed = true;

        if (visualObject != null)
        {
            visualObject.SetActive(true);
        }

        if (revealBurst != null)
        {
            Instantiate(
                revealBurst,
                transform.position,
                Quaternion.identity
            );
        }
    }

    public void ResetReward()
    {
        revealed = false;

        if (visualObject != null)
        {
            visualObject.SetActive(false);
        }
    }
}