using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int coinCount = 0;
    public TextMeshProUGUI coinText;

    [Header("Test Mode")]
    public bool isTestMode = true;

    
    [HideInInspector] public bool isChangingLevel = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);   
            return;
        }
        Instance = this;
    }

    public void SetCoinText(TextMeshProUGUI newCoinText)
    {
        coinText = newCoinText;
        UpdateCoinUI();
    }
   
    public void AddCoin(int amount)
    {
        coinCount += amount;
        UpdateCoinUI();
    }

    void UpdateCoinUI()
    {
        coinText.text = coinCount.ToString();
    }
}
