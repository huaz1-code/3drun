using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 死亡界面管理器
/// 功能：管理死亡界面的显示和隐藏，处理重新开始逻辑
/// </summary>
public class DeathScreenManager : MonoBehaviour
{
    /// <summary>
    /// 死亡界面面板
    /// </summary>
    [Tooltip("死亡界面面板")]
    public GameObject deathPanel;

    /// <summary>
    /// 玩家的Health组件引用
    /// </summary>
    private Health playerHealth;

    /// <summary>
    /// 初始化方法
    /// </summary>
    void Start()
    {
        // 查找玩家对象并获取Health组件
        GameObject player = GameObject.Find("player");
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                // 订阅死亡事件
                playerHealth.OnDeath += OnPlayerDeath;
                Debug.Log("DeathScreenManager: 已订阅玩家死亡事件");
            }
            else
            {
                Debug.LogWarning("DeathScreenManager: 玩家对象上未找到 Health 组件");
            }
        }
        else
        {
            Debug.LogError("DeathScreenManager: 未找到玩家对象");
        }

        // 确保死亡界面一开始是隐藏的
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 玩家死亡时触发
    /// </summary>
    void OnPlayerDeath()
    {
        // 显示死亡界面
        ShowDeathScreen();
    }

    /// <summary>
    /// 显示死亡界面
    /// </summary>
    void ShowDeathScreen()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            Debug.Log("DeathScreenManager: 显示死亡界面");
        }

        // 显示鼠标光标
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// 隐藏死亡界面
    /// </summary>
    void HideDeathScreen()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
            Debug.Log("DeathScreenManager: 隐藏死亡界面");
        }

        // 隐藏鼠标光标
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// 重新开始游戏（按钮点击调用）
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("DeathScreenManager: 重新开始游戏");

        // 恢复时间缩放
        Time.timeScale = 1f;

        // 隐藏死亡界面
        HideDeathScreen();

        // 重置金币计数并刷新UI引用
        CoinUIManager.ResetCoinsAndRefreshUI();

        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 返回主菜单（按钮点击调用）
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("DeathScreenManager: 返回主菜单");

        // 恢复时间缩放
        Time.timeScale = 1f;

        // 重置金币计数并刷新UI引用
        CoinUIManager.ResetCoinsAndRefreshUI();

        // 加载开始场景
        SceneManager.LoadScene("StartScence");
    }

    /// <summary>
    /// 清理事件订阅
    /// </summary>
    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= OnPlayerDeath;
        }
    }
}
