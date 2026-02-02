using UnityEngine;
using UnityEngine.InputSystem; // Fondamentale per Keyboard.current

public class WorldSwitch : MonoBehaviour
{
    public bool canSwitchWorld = true;

    [Header("Worlds")]
    public GameObject realWorld;
    public GameObject fantasyWorld;

    [Header("Camera Settings")]
    public Camera mainCamera;
    public Color realWorldColor = Color.cyan;
    public Color fantasyWorldColor = Color.magenta;

    public static bool isFantasyWorldActive = false;

    void Start()
    {
        // Setup iniziale
        if (realWorld != null) realWorld.SetActive(true);
        if (fantasyWorld != null) fantasyWorld.SetActive(false);

        if (mainCamera != null)
            mainCamera.backgroundColor = realWorldColor;
    }

    void Update()
    {
        // Controlla se il tasto E è stato premuto in questo frame
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleWorld();
        }
    }

    private void ToggleWorld()
    {
        if (!canSwitchWorld) return;

        // Inverte lo stato attuale
        isFantasyWorldActive = !isFantasyWorldActive;

        // Attiva/Disattiva i mondi
        if (realWorld != null) realWorld.SetActive(!isFantasyWorldActive);
        if (fantasyWorld != null) fantasyWorld.SetActive(isFantasyWorldActive);

        // Cambia colore alla camera
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = isFantasyWorldActive ? fantasyWorldColor : realWorldColor;
        }
        
        Debug.Log("Mondo Fantasy Attivo: " + isFantasyWorldActive);
    }
}