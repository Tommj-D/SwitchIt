using UnityEngine;
using UnityEngine.UI;

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
}
