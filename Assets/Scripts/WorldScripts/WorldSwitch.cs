using System.Collections;
using UnityEngine;

public class WorldSwitch : MonoBehaviour
{
    public static WorldSwitch Instance;

    public bool canSwitchWorld = true;
    public bool canSwitchGravity = true;    

    [Header("Worlds")]
    public GameObject realWorld;
    public GameObject fantasyWorld;

    [Header("Camera Settings")]
    public Camera mainCamera;
    public Color realWorldColor = Color.cyan;
    public Color fantasyWorldColor = Color.magenta;

    [Header("Transition")]
    public WorldSwitchTransition transition;

    [Header("MagicFog")]
    public GameObject MagicDust;
    public GameObject MagicFog;

    [HideInInspector] public bool isFantasyWorldActive = false;

    public bool isSwitching = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        // Setup iniziale
        if (realWorld != null) realWorld.SetActive(true);
        if (fantasyWorld != null) fantasyWorld.SetActive(false);
        if (mainCamera != null) mainCamera.backgroundColor = realWorldColor;
        if(MagicDust != null) MagicDust.SetActive(false);
        if(MagicFog != null) MagicFog.SetActive(false);
    }

    public void SwitchWorld()
    {
        if (!canSwitchWorld || isSwitching) return;

        StartCoroutine(SwitchRoutine());
    }

    private IEnumerator SwitchRoutine()
    {
        isSwitching = true;

        if (transition != null)
            yield return transition.PlayTransition(this);

        isSwitching = false;
    }


    public void ApplyWorldChange()
    {
        isFantasyWorldActive = !isFantasyWorldActive;

        if (realWorld != null) realWorld.SetActive(!isFantasyWorldActive);
        if (fantasyWorld != null) fantasyWorld.SetActive(isFantasyWorldActive);

        if (mainCamera != null) mainCamera.backgroundColor = isFantasyWorldActive ? fantasyWorldColor : realWorldColor;

        if (MagicDust != null) MagicDust.SetActive(isFantasyWorldActive);
        if (MagicFog != null) MagicFog.SetActive(isFantasyWorldActive);

        // Aggiorna colori nuvole
        foreach (CloudsManager manager in CloudsManager.AllCloudManagers)
        {
            manager.UpdateCloudDimension(isFantasyWorldActive);
        }
        
        if (isFantasyWorldActive)
        {
            VolumeController.Instance.FadeMixerParam(
                VolumeController.Instance.masterMixer,
                "MusicRealVol",
                -25f,
                0.5f);

            VolumeController.Instance.FadeMixerParam(
                VolumeController.Instance.masterMixer,
                "MusicFantasyVol",
                0f,
                0.5f);
        }
        else
        {
            VolumeController.Instance.FadeMixerParam(
                VolumeController.Instance.masterMixer,
                "MusicRealVol",
                0f,
                0.5f);

            VolumeController.Instance.FadeMixerParam(
                VolumeController.Instance.masterMixer,
                "MusicFantasyVol",
                -25f,
                0.5f);
        }
        // Cambio audio
        if (AudioManager.Instance != null)
        {
            if (isFantasyWorldActive)
                AudioManager.Instance.SetAudioState(AudioManager.AudioState.Fantasy);
            else
                AudioManager.Instance.SetAudioState(AudioManager.AudioState.Normal);
        }
    }
    public void SwitchWorldWithoutAnimation()
    {
        if (!canSwitchWorld || isSwitching) return;
        ApplyWorldChange();
    }
}