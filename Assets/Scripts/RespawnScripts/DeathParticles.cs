using UnityEngine;

public class DeathParticles : MonoBehaviour
{
    public GameObject deathParticles;
    public GameObject player;
    public PlayerRespawn playerRespawn;

    private bool hasSpawned = false;

    // Update is called once per frame
    void Update()
    {
        if (playerRespawn != null && playerRespawn.IsDying() && !hasSpawned)
        {
            Vector3 playerPos = playerRespawn.transform.position;

            if (deathParticles != null)
            {
                Instantiate(deathParticles, playerPos, Quaternion.identity);
                hasSpawned = true;
            }
        }

        if (playerRespawn != null && !playerRespawn.IsDying())
        {
            hasSpawned = false; // reset per la prossima morte
        }
    }
}
