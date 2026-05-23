using UnityEngine;

/// <summary>
/// 血量数据逻辑脚本 - 纯数据层，不涉及任何UI
/// 功能说明：
/// 1. 管理生命值数据（当前值、最大值）
/// 2. 提供受伤、回血、死亡判断等方法
/// 3. 通过事件通知外部状态变化
/// 4. 玩家/小怪/BOSS都可共用
/// </summary>
public class Health : MonoBehaviour
{
    /// <summary>
    /// 最大生命值
    /// </summary>
    [Tooltip("最大生命值")]
    public float maxHealth = 100f;

    /// <summary>
    /// 是否允许超出最大生命值（某些技能可能需要）
    /// </summary>
    [Tooltip("是否允许超出最大生命值")]
    public bool allowOverheal = false;

    /// <summary>
    /// 死亡后暂停游戏的延迟时间（秒）
    /// 0表示立即暂停，大于0表示延迟指定秒数后暂停
    /// </summary>
    [Tooltip("死亡后暂停游戏的延迟时间（秒）")]
    public float deathPauseDelay = 0f;

    /// <summary>
    /// 当前生命值
    /// </summary>
    private float currentHealth;

    /// <summary>
    /// 是否已经死亡
    /// </summary>
    private bool isDead = false;

    /// <summary>
    /// 生命值变化事件 - 当生命值发生变化时触发
    /// 参数：当前生命值, 最大生命值, 变化量
    /// </summary>
    public event System.Action<float, float, float> OnHealthChanged;

    /// <summary>
    /// 受伤事件 - 当受到伤害时触发
    /// 参数：受到的伤害值, 当前生命值, 最大生命值
    /// </summary>
    public event System.Action<float, float, float> OnTakeDamage;

    /// <summary>
    /// 死亡事件 - 当生命值归零时触发
    /// </summary>
    public event System.Action OnDeath;

    /// <summary>
    /// 复活事件 - 当从死亡状态复活时触发
    /// </summary>
    public event System.Action OnRespawn;

    /// <summary>
    /// 初始化方法 - 游戏开始时调用
    /// </summary>
    private void Awake()
    {
        // 初始化当前生命值为最大值
        currentHealth = maxHealth;
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(float damage)
    {
        // 如果已死亡，不处理伤害
        if (isDead) return;

        // 确保伤害值为正数
        damage = Mathf.Max(damage, 0f);

        // 记录伤害前的生命值
        float previousHealth = currentHealth;

        // 减少生命值
        currentHealth = Mathf.Max(currentHealth - damage, 0f);

        // 计算实际变化量
        float changeAmount = currentHealth - previousHealth;

        // 触发生命值变化事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth, changeAmount);

        // 触发受伤事件
        OnTakeDamage?.Invoke(damage, currentHealth, maxHealth);

        // 检查是否死亡
        if (currentHealth <= 0f && !isDead)
        {
            Die();
        }
    }

    /// <summary>
    /// 恢复生命值
    /// </summary>
    /// <param name="healAmount">恢复量</param>
    public void Heal(float healAmount)
    {
        // 如果已死亡，不处理治疗
        if (isDead) return;

        // 确保恢复量为正数
        healAmount = Mathf.Max(healAmount, 0f);

        // 记录治疗前的生命值
        float previousHealth = currentHealth;

        // 增加生命值
        if (allowOverheal)
        {
            // 允许超出最大值
            currentHealth += healAmount;
        }
        else
        {
            // 不允许超出最大值
            currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        }

        // 计算实际变化量
        float changeAmount = currentHealth - previousHealth;

        // 触发生命值变化事件（只在有实际变化时）
        if (changeAmount != 0f)
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth, changeAmount);
        }
    }

    /// <summary>
    /// 设置生命值
    /// </summary>
    /// <param name="newHealth">新的生命值</param>
    public void SetHealth(float newHealth)
    {
        // 记录变化前的生命值
        float previousHealth = currentHealth;

        // 根据是否允许超越来限制生命值
        if (allowOverheal)
        {
            currentHealth = Mathf.Max(newHealth, 0f);
        }
        else
        {
            currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);
        }

        // 计算实际变化量
        float changeAmount = currentHealth - previousHealth;

        // 触发生命值变化事件（只在有实际变化时）
        if (changeAmount != 0f)
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth, changeAmount);
        }

        // 检查死亡状态
        if (currentHealth <= 0f && !isDead)
        {
            Die();
        }
        else if (currentHealth > 0f && isDead)
        {
            isDead = false;
            OnRespawn?.Invoke();
        }
    }

    /// <summary>
    /// 设置最大生命值
    /// </summary>
    /// <param name="newMaxHealth">新的最大生命值</param>
    public void SetMaxHealth(float newMaxHealth)
    {
        // 确保最大生命值大于0
        newMaxHealth = Mathf.Max(newMaxHealth, 1f);

        // 更新最大生命值
        maxHealth = newMaxHealth;

        // 确保当前生命值不超过新的最大值（如果不允许超越）
        if (!allowOverheal)
        {
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        // 触发生命值变化事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth, 0f);
    }

    /// <summary>
    /// 回复满血
    /// </summary>
    public void FullHeal()
    {
        SetHealth(maxHealth);
    }

    /// <summary>
    /// 死亡处理
    /// </summary>
    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();

        if (deathPauseDelay <= 0f)
        {
            // 立即暂停
            Time.timeScale = 0f;
        }
        else
        {
            // 延迟暂停
            StartCoroutine(DelayedPauseCoroutine());
        }
    }

    /// <summary>
    /// 延迟暂停的协程
    /// </summary>
    private System.Collections.IEnumerator DelayedPauseCoroutine()
    {
        yield return new WaitForSeconds(deathPauseDelay);
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 复活
    /// </summary>
    public void Respawn()
    {
        // 恢复生命值为最大值
        currentHealth = maxHealth;
        isDead = false;

        // 触发事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth, maxHealth);
        OnRespawn?.Invoke();
    }

    /// <summary>
    /// 获取当前生命值
    /// </summary>
    /// <returns>当前生命值</returns>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// 获取最大生命值
    /// </summary>
    /// <returns>最大生命值</returns>
    public float GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// 获取生命值百分比（0-1）
    /// </summary>
    /// <returns>生命值百分比</returns>
    public float GetHealthPercent()
    {
        if (maxHealth <= 0f) return 0f;
        return currentHealth / maxHealth;
    }

    /// <summary>
    /// 判断是否死亡
    /// </summary>
    /// <returns>true表示死亡，false表示存活</returns>
    public bool IsDead()
    {
        return isDead;
    }

    /// <summary>
    /// 判断是否满血
    /// </summary>
    /// <returns>true表示满血，false表示不满血</returns>
    public bool IsFullHealth()
    {
        return currentHealth >= maxHealth;
    }

    /// <summary>
    /// 获取剩余生命值
    /// </summary>
    /// <returns>剩余生命值（最大生命值 - 当前生命值）</returns>
    public float GetMissingHealth()
    {
        return maxHealth - currentHealth;
    }
}
