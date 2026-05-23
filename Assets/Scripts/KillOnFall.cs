using UnityEngine;

/// <summary>
/// 玩家掉落死亡检测脚本
/// 功能：当玩家低于初始Plane位置时，直接死亡
/// </summary>
public class KillOnFall : MonoBehaviour
{
    /// <summary>
    /// 初始Plane的Transform引用
    /// 如果不指定，会自动查找名称为"Plane"的对象
    /// </summary>
    [Tooltip("初始Plane对象")]
    public Transform initialPlane;

    /// <summary>
    /// Plane的Y位置（作为死亡界限）
    /// </summary>
    private float deathY;

    /// <summary>
    /// 玩家的Health组件引用
    /// </summary>
    private Health health;

    /// <summary>
    /// 是否已经检测到掉落死亡（防止重复触发）
    /// </summary>
    private bool hasDiedFromFall = false;

    /// <summary>
    /// 初始化方法
    /// </summary>
    void Start()
    {
        // 获取玩家的Health组件
        health = GetComponent<Health>();
        if (health == null)
        {
            Debug.LogWarning("KillOnFall: 未找到 Health 组件，请确保玩家对象上挂载了 Health.cs");
        }

        // 如果没有指定Plane，尝试查找场景中的Plane对象
        if (initialPlane == null)
        {
            GameObject planeObj = GameObject.Find("Plane");
            if (planeObj != null)
            {
                initialPlane = planeObj.transform;
                Debug.Log("KillOnFall: 找到Plane对象");
            }
            else
            {
                Debug.LogError("KillOnFall: 未找到Plane对象，请在Inspector中手动指定");
                enabled = false;
                return;
            }
        }

        // 记录Plane的Y位置作为死亡界限
        deathY = initialPlane.position.y;
        Debug.Log($"KillOnFall: 死亡界限设置为 Y = {deathY}");
    }

    /// <summary>
    /// 更新方法 - 每帧检测玩家位置
    /// </summary>
    void Update()
    {
        // 如果已经因掉落死亡过，不再检测
        if (hasDiedFromFall) return;

        // 如果没有Health组件，不处理
        if (health == null) return;

        // 如果玩家已经死亡，不处理
        if (health.IsDead()) return;

        // 检查玩家是否低于Plane位置
        if (transform.position.y < deathY)
        {
            // 标记已因掉落死亡
            hasDiedFromFall = true;

            // 直接杀死玩家
            health.SetHealth(0f);
            Debug.Log("KillOnFall: 玩家掉落死亡！");
        }
    }

    /// <summary>
    /// 重置掉落死亡状态（用于复活）
    /// </summary>
    public void ResetFallDeath()
    {
        hasDiedFromFall = false;
    }
}
