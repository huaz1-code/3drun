using UnityEngine;

public class PotionEffects : MonoBehaviour
{
    [Header("生命药水设置")]
    [Tooltip("生命药水恢复量")]
    public float healthPotionHealAmount = 50f;

    [Header("迅捷药水设置")]
    [Tooltip("迅捷药水速度加成百分比")]
    public float speedPotionBonusPercent = 0.5f;

    [Tooltip("迅捷药水持续时间（秒）")]
    public float speedDuration = 5f;

    [Header("护盾药水设置")]
    [Tooltip("护盾持续时间（秒）")]
    public float shieldDuration = 10f;

    private Health health;
    private PlayerController playerController;
    private float originalMoveSpeed;
    private bool isSpeedBoosted = false;
    private bool isShieldActive = false;
    private float shieldEndTime;
    private float speedEndTime;

    private void Awake()
    {
        health = GetComponent<Health>();
        playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            originalMoveSpeed = playerController.moveSpeed;
        }
    }

    private void Update()
    {
        if (isShieldActive && Time.time >= shieldEndTime)
        {
            DeactivateShield();
        }

        if (isSpeedBoosted && Time.time >= speedEndTime)
        {
            DeactivateSpeedBoost();
        }

        // 按1使用生命药水
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            UseHealthPotion();
        }

        // 按2使用护盾药水
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            UseShieldPotion();
        }

        // 按3使用迅捷药水
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            UseSpeedPotion();
        }
    }

    /// <summary>
    /// 使用生命药水 - 恢复50点生命值
    /// </summary>
    public void UseHealthPotion()
    {
        if (health == null)
        {
            Debug.LogWarning("PotionEffects: 未找到 Health 组件");
            return;
        }

        if (PotionInventory.Instance == null || PotionInventory.Instance.healthPotion <= 0)
        {
            Debug.Log("没有生命药水！");
            return;
        }

        PotionInventory.Instance.healthPotion--;
        health.Heal(healthPotionHealAmount);
        Debug.Log($"使用生命药水，恢复 {healthPotionHealAmount} 点生命值");
    }

    /// <summary>
    /// 使用迅捷药水 - 速度提高50%，持续5秒
    /// </summary>
    public void UseSpeedPotion()
    {
        if (playerController == null)
        {
            Debug.LogWarning("PotionEffects: 未找到 PlayerController 组件");
            return;
        }

        if (PotionInventory.Instance == null || PotionInventory.Instance.speedPotion <= 0)
        {
            Debug.Log("没有迅捷药水！");
            return;
        }

        PotionInventory.Instance.speedPotion--;

        if (!isSpeedBoosted)
        {
            isSpeedBoosted = true;
            playerController.moveSpeed = originalMoveSpeed * (1f + speedPotionBonusPercent);
            Debug.Log($"使用迅捷药水，速度提高 {speedPotionBonusPercent * 100}%，持续{speedDuration}秒");
        }

        speedEndTime = Time.time + speedDuration;
        Debug.Log($"迅捷药水持续时间刷新至{speedDuration}秒");
    }

    /// <summary>
    /// 使用护盾药水 - 10秒内抵挡一次伤害
    /// </summary>
    public void UseShieldPotion()
    {
        if (PotionInventory.Instance == null || PotionInventory.Instance.shieldPotion <= 0)
        {
            Debug.Log("没有护盾药水！");
            return;
        }

        PotionInventory.Instance.shieldPotion--;

        if (!isShieldActive)
        {
            isShieldActive = true;
            shieldEndTime = Time.time + shieldDuration;
            Debug.Log($"使用护盾药水，{shieldDuration}秒内抵挡一次伤害");
        }
        else
        {
            shieldEndTime = Time.time + shieldDuration;
            Debug.Log($"护盾已存在，刷新护盾持续时间");
        }
    }

    /// <summary>
    /// 检查并消耗护盾 - 返回true表示伤害被护盾抵挡（抵挡后护盾失效）
    /// </summary>
    public bool CheckAndConsumeShield()
    {
        if (isShieldActive)
        {
            DeactivateShield();
            Debug.Log("护盾抵挡了一次伤害！");
            return true;
        }
        return false;
    }

    private void DeactivateShield()
    {
        isShieldActive = false;
        Debug.Log("护盾效果结束");
    }

    private void DeactivateSpeedBoost()
    {
        isSpeedBoosted = false;
        if (playerController != null)
        {
            playerController.moveSpeed = originalMoveSpeed;
        }
        Debug.Log("迅捷药水效果结束，速度恢复正常");
    }

    public bool IsShieldActive()
    {
        return isShieldActive;
    }

    public bool IsSpeedBoosted()
    {
        return isSpeedBoosted;
    }
}
