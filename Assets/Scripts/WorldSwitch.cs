using System; //per action<T>
using UnityEngine;

public class WorldSwitch : MonoBehaviour
{
    private PlayerControls controls; // riferimento al sistema di input

    [SerializeField] private GameObject realWorld;
    [SerializeField] private GameObject fantasyWorld;
    
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Color realWorldColor = Color.cyan;
    [SerializeField] private Color fantasyWorldColor = Color.magenta;

    public static bool isFantasyWorldActive = false;

    // EVENTO: notifico agli listener quale mondo è attivo
    public static event Action<bool> OnWorldChanged;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //All'avvio del gioco, il mondo reale è attivo e il mondo fantasy è inattivo
        realWorld.SetActive(true);
        fantasyWorld.SetActive(false);

        // Imposta il colore iniziale della camera
        if (mainCamera != null)
            mainCamera.backgroundColor = realWorldColor;

        // Notifica lo stato iniziale agli eventuali listener
        OnWorldChanged?.Invoke(isFantasyWorldActive);
    }

    // Update is called once per frame
    void Update()
    {
        if(controls.Player.SwitchWorld.WasPressedThisFrame())
        {
            if(isFantasyWorldActive)
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
            // INVIO EVENTO: notifico tutti gli oggetti interessati
            OnWorldChanged?.Invoke(isFantasyWorldActive);
        }
    }
}
