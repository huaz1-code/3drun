using UnityEngine;

/// <summary>
/// 玩家离开后销毁平台脚本
/// 功能：当玩家第一次离开此平台后，自动销毁该平台
/// </summary>
public class DestroyOnPlayerLeave : MonoBehaviour
{
    /// <summary>
    /// 是否已经检测到玩家离开过
    /// 防止多次触发销毁
    /// </summary>
    private bool hasLeft = false;

    /// <summary>
    /// 当玩家离开平台碰撞体时调用
    /// </summary>
    /// <param name="other">离开的碰撞体</param>
    void OnTriggerExit(Collider other)
    {
        // 如果已经离开过，不再处理
        if (hasLeft) return;

        // 检查离开的对象是否是玩家
        if (other.CompareTag("Player") || other.gameObject.name == "player")
        {
            // 标记已离开
            hasLeft = true;
            
            // 销毁平台
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 当玩家离开平台碰撞时调用（非触发器模式）
    /// </summary>
    /// <param name="other">离开的碰撞信息</param>
    void OnCollisionExit(Collision other)
    {
        // 如果已经离开过，不再处理
        if (hasLeft) return;

        // 检查离开的对象是否是玩家
        if (other.gameObject.CompareTag("Player") || other.gameObject.name == "player")
        {
            // 标记已离开
            hasLeft = true;
            
            // 销毁平台
            Destroy(gameObject);
        }
    }
}
