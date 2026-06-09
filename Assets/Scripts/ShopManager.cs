using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Tooltip("NeedCounter文本组件")]
    public TextMeshProUGUI needCounterText;

    private int needCounter = 1;

    private void Start()
    {
        UpdateNeedCounterText();
    }

    public void OnBuyHealthPotion()
    {
        if (CoinUIManager.SubtractCoins(needCounter))
        {
            PotionInventory.Instance?.AddHealthPotion();
            DoubleNeedCounter();
        }
        else
        {
            Debug.Log("金币不足！");
        }
    }

    public void OnBuyShieldPotion()
    {
        if (CoinUIManager.SubtractCoins(needCounter))
        {
            PotionInventory.Instance?.AddShieldPotion();
            DoubleNeedCounter();
        }
        else
        {
            Debug.Log("金币不足！");
        }
    }

    public void OnBuySpeedPotion()
    {
        if (CoinUIManager.SubtractCoins(needCounter))
        {
            PotionInventory.Instance?.AddSpeedPotion();
            DoubleNeedCounter();
        }
        else
        {
            Debug.Log("金币不足！");
        }
    }

    private void DoubleNeedCounter()
    {
        needCounter *= 2;
        UpdateNeedCounterText();
    }

    private void UpdateNeedCounterText()
    {
        if (needCounterText != null)
        {
            needCounterText.text = needCounter.ToString();
        }
    }
}
