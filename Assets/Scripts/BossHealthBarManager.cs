using UnityEngine;

/// <summary>
/// Boss血条管理器
/// 功能：管理Boss血条的显示和隐藏，绑定Boss的Health组件
/// </summary>
public class BossHealthBarManager : MonoBehaviour
{
    /// <summary>
    /// Boss血条面板
    /// </summary>
    [Tooltip("Boss血条面板（包含背景和血条）")]
    public GameObject bossHealthBarPanel;

    /// <summary>
    /// Boss Health组件引用
    /// </summary>
    private Health bossHealth;

    /// <summary>
    /// Boss HealthBar组件引用
    /// </summary>
    private HealthBar healthBar;

    /// <summary>
    /// Boss对象引用
    /// </summary>
    private GameObject bossObject;

    /// <summary>
    /// 是否已经初始化
    /// </summary>
    private bool isInitialized = false;

    /// <summary>
    /// 初始化Boss血条系统
    /// 需要在BossTrigger激活Boss后调用
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;

        // 查找Boss对象
        if (bossObject == null)
        {
            bossObject = GameObject.Find("Boss");
        }

        if (bossObject == null)
        {
            Debug.LogError("BossHealthBarManager: 未找到Boss对象！");
            return;
        }

        // 获取Boss的Health组件
        bossHealth = bossObject.GetComponent<Health>();
        if (bossHealth == null)
        {
            Debug.LogError("BossHealthBarManager: Boss对象上没有Health组件！");
            return;
        }

        // 获取Boss的HealthBar组件
        healthBar = bossObject.GetComponent<HealthBar>();
        if (healthBar == null)
        {
            Debug.LogError("BossHealthBarManager: Boss对象上没有HealthBar组件！请在Boss对象上添加HealthBar脚本。");
            return;
        }

        // 确保血条一开始是隐藏的
        HideBossHealthBar();

        // 订阅Boss死亡事件
        bossHealth.OnDeath += OnBossDeath;

        isInitialized = true;
        Debug.Log("BossHealthBarManager: Boss血条系统初始化完成");
    }

    /// <summary>
    /// 显示Boss血条
    /// </summary>
    public void ShowBossHealthBar()
    {
        if (bossHealthBarPanel != null)
        {
            bossHealthBarPanel.SetActive(true);
            Debug.Log("BossHealthBarManager: 显示Boss血条");
        }
        else
        {
            Debug.LogWarning("BossHealthBarManager: 未设置Boss血条面板！");
        }
    }

    /// <summary>
    /// 隐藏Boss血条
    /// </summary>
    public void HideBossHealthBar()
    {
        if (bossHealthBarPanel != null)
        {
            bossHealthBarPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Boss死亡回调
    /// </summary>
    private void OnBossDeath()
    {
        Debug.Log("BossHealthBarManager: Boss已死亡，隐藏血条");
        HideBossHealthBar();
    }

    /// <summary>
    /// 清理事件订阅
    /// </summary>
    private void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.OnDeath -= OnBossDeath;
        }
    }

    /// <summary>
    /// 手动设置Boss对象
    /// </summary>
    /// <param name="boss">Boss对象</param>
    public void SetBossObject(GameObject boss)
    {
        bossObject = boss;
        isInitialized = false;
    }
}
