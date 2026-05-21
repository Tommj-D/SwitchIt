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
        fantasyFog.SetActive(false);
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerInside = false;

        fantasyFog.SetActive(false);
        realFog.SetActive(false);

        if (realWorldDust != null)
            realWorldDust.SetActive(false);
    }

    private void Update()
    {
        if (!playerInside) return;

        UpdateBossRoomVisuals();
    }

    public void UpdateBossRoomVisuals()
    {
        bool isFantasy = WorldSwitch.Instance.isFantasyWorldActive;

        // Fog
        fantasyFog.SetActive(isFantasy);
        realFog.SetActive(!isFantasy);

        // Dust reale SOLO nella boss room
        if (realWorldDust != null)
        {
            realWorldDust.SetActive(!isFantasy);
        }
    }
}