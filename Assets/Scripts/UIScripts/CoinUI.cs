using UnityEngine;
using TMPro;
public class CoinUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCoinText(coinText);
        }
    }
}
