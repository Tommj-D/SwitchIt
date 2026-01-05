using System;
using UnityEngine;
using UnityEngine.InputSystem;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //All'avvio del gioco, il mondo reale è attivo e il mondo fantasy è inattivo
        realWorld.SetActive(true);
        fantasyWorld.SetActive(false);

        // Imposta il colore iniziale della camera
        if (mainCamera != null)
            mainCamera.backgroundColor = realWorldColor;
    }

    //cambio mondo
    public void Switch(InputAction.CallbackContext context)
    {
        if (!canSwitchWorld)
            return;

        if (isFantasyWorldActive)
        {
            //Passa al mondo reale
            realWorld.SetActive(true);
            fantasyWorld.SetActive(false);
            isFantasyWorldActive = false;
            if (mainCamera != null)
                mainCamera.backgroundColor = realWorldColor;

        }
        else
        {
            //Passa al mondo fantasy
            realWorld.SetActive(false);
            fantasyWorld.SetActive(true);
            isFantasyWorldActive = true;
            if (mainCamera != null)
                mainCamera.backgroundColor = fantasyWorldColor;
        }
    }
}
