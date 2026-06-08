using UnityEngine;

public class PotionInventory : MonoBehaviour
{
    [Tooltip("生命药水数量")]
    public int healthPotion = 0;

    [Tooltip("护盾药水数量")]
    public int shieldPotion = 0;

    [Tooltip("迅捷药水数量")]
    public int speedPotion = 0;

    public static PotionInventory Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddHealthPotion()
    {
        healthPotion++;
        Debug.Log($"获得生命药水！当前数量：{healthPotion}");
    }

    public void AddShieldPotion()
    {
        shieldPotion++;
        Debug.Log($"获得护盾药水！当前数量：{shieldPotion}");
    }

    public void AddSpeedPotion()
    {
        speedPotion++;
        Debug.Log($"获得迅捷药水！当前数量：{speedPotion}");
    }
}
