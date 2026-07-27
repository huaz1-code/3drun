using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryPanelManager : MonoBehaviour
{
    [Tooltip("胜利面板对象")]
    public GameObject victoryPanel;

    private Health bossHealth;

    void Start()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        FindBossAndSubscribe();
    }

    void FindBossAndSubscribe()
    {
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        if (boss != null)
        {
            bossHealth = boss.GetComponent<Health>();
            if (bossHealth != null)
            {
                bossHealth.OnDeath += OnBossDeath;
                Debug.Log("VictoryPanelManager: 已订阅Boss死亡事件");
            }
            else
            {
                Debug.LogWarning("VictoryPanelManager: Boss对象上未找到 Health 组件");
            }
        }
        else
        {
            Debug.LogWarning("VictoryPanelManager: 未找到Boss对象，稍后尝试查找");
        }
    }

    void Update()
    {
        if (bossHealth == null)
        {
            FindBossAndSubscribe();
        }
    }

    void OnBossDeath()
    {
        ShowVictoryScreen();
    }

    void ShowVictoryScreen()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Debug.Log("VictoryPanelManager: 显示胜利界面");
        }

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("VictoryPanelManager: 重新开始游戏");
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScence");
        Debug.Log("VictoryPanelManager: 返回主菜单");
    }

    void OnDestroy()
    {
        if (bossHealth != null)
        {
            bossHealth.OnDeath -= OnBossDeath;
        }
    }
}