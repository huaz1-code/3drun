using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public void OnBuyHealthPotion()
    {
        if (PotionInventory.Instance != null)
        {
            PotionInventory.Instance.AddHealthPotion();
        }
        else
        {
            Debug.LogWarning("PotionInventory 未找到！请确保场景中存在 PotionInventory 组件。");
        }
    }

    public void OnBuyShieldPotion()
    {
        if (PotionInventory.Instance != null)
        {
            PotionInventory.Instance.AddShieldPotion();
        }
        else
        {
            Debug.LogWarning("PotionInventory 未找到！请确保场景中存在 PotionInventory 组件。");
        }
    }

    public void OnBuySpeedPotion()
    {
        if (PotionInventory.Instance != null)
        {
            PotionInventory.Instance.AddSpeedPotion();
        }
        else
        {
            Debug.LogWarning("PotionInventory 未找到！请确保场景中存在 PotionInventory 组件。");
        }
    }
}
