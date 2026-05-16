using UnityEngine;

/// <summary>
/// NPC生成器脚本 - 负责在平台上生成NPC
/// 功能说明：
/// 1. 根据平台索引决定是否生成NPC
/// 2. 在符合条件的平台上生成NPC
/// 3. 设置NPC与平台的关联
/// </summary>
public class NPCSpawner : MonoBehaviour
{
    /// <summary>
    /// NPC高度偏移 - NPC在平台上方的垂直距离
    /// 正值表示在平台上方
    /// </summary>
    [Tooltip("NPC在平台上方的偏移")]
    public float npcHeightOffset = 1.5f;

    /// <summary>
    /// NPC生成间隔 - 每隔几个平台生成一个NPC
    /// 值为10表示每10个平台生成1个NPC
    /// </summary>
    [Tooltip("NPC生成间隔（每几个平台生成一个）")]
    public int spawnInterval = 10;

    /// <summary>
    /// NPC开始生成的平台索引 - 前几个平台不生成NPC
    /// 用于给玩家留出初始安全区域
    /// </summary>
    [Tooltip("NPC开始生成的平台索引")]
    public int startSpawnIndex = 10;

    /// <summary>
    /// 在平台上生成NPC - 根据平台索引决定是否生成
    /// 由PlatformSpawner在生成新平台后调用
    /// </summary>
    /// <param name="platform">要生成NPC的平台</param>
    /// <param name="platformIndex">平台的索引编号</param>
    public void SpawnNPCOnPlatform(GameObject platform, int platformIndex)
    {
        // ========== 检查是否符合生成条件 ==========
        // 条件1：平台索引必须大于起始索引
        // 条件2：平台索引必须是生成间隔的倍数
        if (platformIndex < startSpawnIndex || platformIndex % spawnInterval != 0)
            return;

        // 检查NPC池是否可用
        if (NPCPool.Instance == null) return;

        // 获取平台的Renderer组件（用于获取边界）
        var renderer = platform.GetComponent<Renderer>();
        if (renderer == null) return;

        // 获取平台的碰撞边界
        var bounds = renderer.bounds;

        // ========== 计算NPC生成位置 ==========
        // 在平台范围内随机选择X和Z坐标
        // 留出边缘空白区域（1f）
        float randomX = Random.Range(bounds.min.x + 1f, bounds.max.x - 1f);
        float randomZ = Random.Range(bounds.min.z + 1f, bounds.max.z - 1f);

        // NPC位置在平台上方
        float y = bounds.max.y + npcHeightOffset;

        // 构建生成位置
        Vector3 spawnPosition = new Vector3(randomX, y, randomZ);

        // ========== 从对象池获取NPC ==========
        GameObject npc = NPCPool.Instance.GetNPC(spawnPosition);

        if (npc != null)
        {
            // 获取平台的Platform组件
            Platform platformComp = platform.GetComponent<Platform>();
            if (platformComp != null)
            {
                // 标记平台有NPC
                platformComp.HasNPC = true;

                // 设置NPC引用
                platformComp.SetNPC(npc);
            }
        }
    }
}
