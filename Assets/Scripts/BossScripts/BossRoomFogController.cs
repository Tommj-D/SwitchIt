using UnityEngine;

public class BossRoomFogController : MonoBehaviour
{
    [Header("Boss Fog")]
    [SerializeField] private GameObject fantasyFog;
    [SerializeField] private GameObject realFog;

    [Header("Boss Dust")]
    [SerializeField] private GameObject realWorldDust;

    private bool playerInside = false;

    private void Start()
    {
        if (fantasyFog != null)
            fantasyFog.SetActive(false);

        if (realFog != null)
            realFog.SetActive(false);

        if (realWorldDust != null)
            realWorldDust.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerInside = true;

        // Disattiva permanentemente la fog globale
        WorldSwitch.Instance.disableGlobalMagicFog = true;

        if (WorldSwitch.Instance.MagicFog != null)
        {
            WorldSwitch.Instance.MagicFog.SetActive(false);
        }

        UpdateBossRoomVisuals();
    }

    /*private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision || !collision.CompareTag("Player")) return;

        playerInside = false;

        if (fantasyFog != null)
            fantasyFog.SetActive(false);

        if (realFog != null)
            realFog.SetActive(false);

        if (realWorldDust != null)
            realWorldDust.SetActive(false);
    }*/

    private void Update()
    {
        if (!playerInside) return;

        UpdateBossRoomVisuals();
    }

    public void UpdateBossRoomVisuals()
    {
        if (WorldSwitch.Instance == null) return;

        bool isFantasy = WorldSwitch.Instance.isFantasyWorldActive;

        if (fantasyFog != null)
            fantasyFog.SetActive(isFantasy);

        if (realFog != null)
            realFog.SetActive(!isFantasy);

        if (realWorldDust != null)
            realWorldDust.SetActive(!isFantasy);
    }
}