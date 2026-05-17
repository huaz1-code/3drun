using UnityEngine;

/// <summary>
/// 简单NPC脚本 - 第二个平台专属NPC
/// 功能说明：
/// 1. 在第二个平台生成
/// 2. 跟随玩家移动
/// 3. 平台消失时自动销毁
/// </summary>
public class SimpleNPC : MonoBehaviour
{
    /// <summary>
    /// 移动速度 - NPC每秒移动的距离
    /// </summary>
    [Tooltip("移动速度")]
    public float moveSpeed = 3f;

    /// <summary>
    /// 玩家引用 - 用于跟随玩家
    /// </summary>
    private Transform player;

    /// <summary>
    /// 初始化方法 - 游戏开始时调用
    /// </summary>
    void Start()
    {
        // 查找玩家对象
        GameObject playerObj = GameObject.Find("player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    /// <summary>
    /// 更新方法 - 每帧调用
    /// </summary>
    void Update()
    {
        // 如果没有玩家引用，不移动
        if (player == null) return;

        // 计算朝向玩家的方向（只考虑X和Z轴，保持Y轴不变）
        Vector3 direction = new Vector3(
            player.position.x - transform.position.x,
            0,
            player.position.z - transform.position.z
        ).normalized;

        // 如果方向有效，移动并转向
        if (direction.magnitude > 0.01f)
        {
            // 移动向玩家
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

            // 转向移动方向
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                5f * Time.deltaTime
            );
        }
    }
}