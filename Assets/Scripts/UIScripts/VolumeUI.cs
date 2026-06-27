using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // FONDAMENTALE: Aggiunto per poter cambiare le scene!

public class VolumeUI : MonoBehaviour
{
    [SerializeField] private GameObject volumeMenuPanel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Awake()
    {
        if (VolumeController.Instance != null)
        {
            VolumeController.Instance.SetUI(
                volumeMenuPanel,
                musicSlider,
                sfxSlider
            );
        }
    }

    private void OnDestroy()
    {
        if (VolumeController.Instance != null)
        {
            VolumeController.Instance.ClearUI(volumeMenuPanel);
        }
    }

    public void LoadLevelSelection()
    {
        Time.timeScale = 1f; 

        if (VolumeController.Instance != null)
        {
            VolumeController.Instance.StopMusic();
        }

        SceneManager.LoadScene("Menu"); 
    }
}