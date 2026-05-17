using UnityEngine;

public class RewardTarget : MonoBehaviour
{
    [Header("Oggetto Da Mostrare")]
    [SerializeField] private GameObject objectToReveal;

    [Header("Effetti")]
    [SerializeField] private ParticleSystem revealBurst;

    private void Awake()
    {
        if (objectToReveal != null)
        {
            objectToReveal.SetActive(false);
        }
    }

    public void Reveal()
    {
        if (objectToReveal != null)
        {
            objectToReveal.SetActive(true);
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
}