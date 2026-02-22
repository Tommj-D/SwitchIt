using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

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

    [Header("Audio")]
    public AudioSource musicSource;
    public AudioMixerGroup musicRealGroup;
    public AudioMixerGroup musicFantasyGroup;

    [HideInInspector] public bool isFantasyWorldActive = false;

    private bool isSwitching = false;

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

        // Cambio audio
        if (musicSource != null)
        {
            musicSource.outputAudioMixerGroup =
                isFantasyWorldActive ? musicFantasyGroup : musicRealGroup;
        }
    }

    public void SwitchWorldWithoutAnimation()
    {
        if (!canSwitchWorld || isSwitching) return;
        ApplyWorldChange();
    }
}