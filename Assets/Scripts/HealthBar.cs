using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 血条UI显示脚本 - 纯UI层，只负责血条显示
/// 功能说明：
/// 1. 通过事件监听 Health 组件的生命值变化
/// 2. 更新即时血条和缓冲血条的显示
/// 3. 支持玩家/小怪/BOSS共用
/// </summary>
public class HealthBar : MonoBehaviour
{
    /// <summary>
    /// 即时血条 - 立即响应生命值变化
    /// </summary>
    [Tooltip("即时血条（立即响应）")]
    public Image hpBar;

    /// <summary>
    /// 缓冲血条 - 延迟跟随即时血条
    /// </summary>
    [Tooltip("缓冲血条（延迟跟随）")]
    public Image delayBar;

    /// <summary>
    /// 缓冲延迟时间 - 缓冲条开始跟随前的等待时间（秒）
    /// </summary>
    [Tooltip("缓冲延迟时间（秒）")]
    public float delayTime = 0.2f;

    /// <summary>
    /// 缓冲动画时长 - 缓冲条从当前值插值到目标值所需时间（秒）
    /// </summary>
    [Tooltip("缓冲动画时长（秒）")]
    public float lerpDuration = 0.25f;

    /// <summary>
    /// 是否自动查找目标对象上的 Health 组件
    /// </summary>
    [Tooltip("是否自动查找Health组件")]
    public bool autoFindHealth = true;

    /// <summary>
    /// 目标 Health 组件引用（如果不自动查找则手动指定）
    /// </summary>
    [Tooltip("目标Health组件（不自动查找时使用）")]
    public Health targetHealth;

    /// <summary>
    /// 当前生命值百分比（0-1）
    /// </summary>
    private float currentFill = 1f;

    /// <summary>
    /// 缓冲协程引用
    /// </summary>
    private Coroutine delayCoroutine;

    /// <summary>
    /// 初始化方法
    /// </summary>
    private void Awake()
    {
        // 初始化血条显示
        if (hpBar != null)
            hpBar.fillAmount = currentFill;

        if (delayBar != null)
            delayBar.fillAmount = currentFill;
    }

    /// <summary>
    /// 启动方法
    /// </summary>
    private void Start()
    {
        // 如果自动查找，则查找父对象或目标对象上的 Health 组件
        if (autoFindHealth)
        {
            FindAndBindHealth();
        }
        else if (targetHealth != null)
        {
            // 手动指定了 Health 组件，绑定事件
            BindHealthEvents(targetHealth);
        }
    }

    /// <summary>
    /// 查找并绑定 Health 组件
    /// </summary>
    private void FindAndBindHealth()
    {
        // 首先尝试在当前对象上查找
        Health health = GetComponent<Health>();

        // 如果没找到，尝试在父对象上查找
        if (health == null)
        {
            health = GetComponentInParent<Health>();
        }

        // 如果还是没找到，尝试查找指定的目标
        if (health == null && targetHealth != null)
        {
            health = targetHealth;
        }

        // 如果找到了 Health 组件
        if (health != null)
        {
            targetHealth = health;
            BindHealthEvents(health);

            // 初始化血条显示
            UpdateHealthBar(health.GetCurrentHealth(), health.GetMaxHealth());
        }
        else
        {
            Debug.LogWarning("HealthBar: 未找到 Health 组件，请检查设置");
        }
    }

    /// <summary>
    /// 绑定 Health 组件的事件
    /// </summary>
    /// <param name="health">目标 Health 组件</param>
    public void BindHealthEvents(Health health)
    {
        if (health == null) return;

        // 订阅生命值变化事件
        health.OnHealthChanged += OnHealthChanged;

        // 订阅受伤事件（可选，可以用于播放受伤特效等）
        health.OnTakeDamage += OnTakeDamage;

        // 订阅死亡事件（可选，可以用于隐藏血条等）
        health.OnDeath += OnDeath;

        // 订阅复活事件（可选，可以用于显示血条等）
        health.OnRespawn += OnRespawn;
    }

    /// <summary>
    /// 解绑 Health 组件的事件
    /// </summary>
    /// <param name="health">目标 Health 组件</param>
    public void UnbindHealthEvents(Health health)
    {
        if (health == null) return;

        health.OnHealthChanged -= OnHealthChanged;
        health.OnTakeDamage -= OnTakeDamage;
        health.OnDeath -= OnDeath;
        health.OnRespawn -= OnRespawn;
    }

    /// <summary>
    /// 生命值变化回调
    /// </summary>
    /// <param name="currentHealth">当前生命值</param>
    /// <param name="maxHealth">最大生命值</param>
    /// <param name="changeAmount">变化量</param>
    private void OnHealthChanged(float currentHealth, float maxHealth, float changeAmount)
    {
        UpdateHealthBar(currentHealth, maxHealth);
    }

    /// <summary>
    /// 受伤回调
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="currentHealth">当前生命值</param>
    /// <param name="maxHealth">最大生命值</param>
    private void OnTakeDamage(float damage, float currentHealth, float maxHealth)
    {
        // 可以在这里添加受伤特效、抖动等效果
    }

    /// <summary>
    /// 死亡回调
    /// </summary>
    private void OnDeath()
    {
        // 可以在这里隐藏血条或添加死亡特效
        // gameObject.SetActive(false);
    }

    /// <summary>
    /// 复活回调
    /// </summary>
    private void OnRespawn()
    {
        // 可以在这里重新显示血条
        // gameObject.SetActive(true);
    }

    /// <summary>
    /// 更新血条显示
    /// </summary>
    /// <param name="currentHealth">当前生命值</param>
    /// <param name="maxHealth">最大生命值</param>
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        // 计算填充比例
        float targetFill = maxHealth > 0f ? currentHealth / maxHealth : 0f;
        targetFill = Mathf.Clamp01(targetFill);

        // 更新当前填充值
        currentFill = targetFill;

        // 更新即时血条
        if (hpBar != null)
        {
            hpBar.fillAmount = targetFill;
        }

        // 更新缓冲血条（通过协程实现延迟跟随）
        UpdateDelayBar(targetFill);
    }

    /// <summary>
    /// 更新缓冲血条
    /// </summary>
    /// <param name="targetFill">目标填充比例</param>
    private void UpdateDelayBar(float targetFill)
    {
        // 如果没有缓冲条，直接返回
        if (delayBar == null) return;

        // 如果有正在运行的协程，先停止
        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
        }

        // 启动缓冲协程
        delayCoroutine = StartCoroutine(DelayBarLerp(targetFill));
    }

    /// <summary>
    /// 缓冲条插值协程
    /// </summary>
    /// <param name="targetFill">目标填充比例</param>
    /// <returns>协程迭代器</returns>
    private IEnumerator DelayBarLerp(float targetFill)
    {
        // 等待延迟时间
        yield return new WaitForSeconds(delayTime);

        // 记录起始填充值
        float startFill = delayBar.fillAmount;

        // 插值过渡
        for (float t = 0; t < lerpDuration; t += Time.deltaTime)
        {
            delayBar.fillAmount = Mathf.Lerp(startFill, targetFill, t / lerpDuration);
            yield return null;
        }

        // 确保最终值准确
        delayBar.fillAmount = targetFill;
    }

    /// <summary>
    /// 设置目标 Health 组件
    /// </summary>
    /// <param name="health">目标 Health 组件</param>
    public void SetTargetHealth(Health health)
    {
        // 先解绑之前的事件
        if (targetHealth != null)
        {
            UnbindHealthEvents(targetHealth);
        }

        // 设置新的目标
        targetHealth = health;
        autoFindHealth = false;

        // 绑定新的事件
        if (targetHealth != null)
        {
            BindHealthEvents(targetHealth);
            UpdateHealthBar(targetHealth.GetCurrentHealth(), targetHealth.GetMaxHealth());
        }
    }

    /// <summary>
    /// 手动更新血条（直接设置填充比例）
    /// </summary>
    /// <param name="fillAmount">填充比例（0-1）</param>
    public void SetFillAmount(float fillAmount)
    {
        fillAmount = Mathf.Clamp01(fillAmount);
        currentFill = fillAmount;

        if (hpBar != null)
            hpBar.fillAmount = fillAmount;

        if (delayBar != null)
            delayBar.fillAmount = fillAmount;
    }

    /// <summary>
    /// 获取当前填充比例
    /// </summary>
    /// <returns>当前填充比例（0-1）</returns>
    public float GetFillAmount()
    {
        return currentFill;
    }

    /// <summary>
    /// 清理方法
    /// </summary>
    private void OnDestroy()
    {
        // 解绑事件，防止内存泄漏
        if (targetHealth != null)
        {
            UnbindHealthEvents(targetHealth);
        }
    }
}
