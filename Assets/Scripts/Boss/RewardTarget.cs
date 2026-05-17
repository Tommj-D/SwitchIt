using UnityEngine;

public class RewardTarget : MonoBehaviour
{
    [Header("Effetti")]
    [SerializeField] private ParticleSystem revealBurst;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Reveal()
    {
        gameObject.SetActive(true);

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