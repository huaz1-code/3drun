using UnityEngine;

/// <summary>
/// 简单NPC生成器脚本 - 负责在第二个平台生成NPC
/// 功能说明：
/// 1. 在第二个平台（索引为1）生成NPC
/// 2. 不使用对象池，直接创建
/// 3. 平台消失时NPC自动销毁
/// </summary>
public class SimpleNPCSpawner : MonoBehaviour
{
    /// <summary>
    /// NPC预制体 - 用于创建NPC的模板
    /// </summary>
    [Tooltip("NPC预制体")]
    public GameObject npcPrefab;

    /// <summary>
    /// NPC高度偏移 - NPC在平台上方的垂直距离
    /// </summary>
    [Tooltip("NPC在平台上方的偏移")]
    public float npcHeightOffset = 1.5f;

    /// <summary>
    /// 目标平台索引 - 只在这个索引的平台生成NPC
    /// 设置为1表示第二个平台（索引从0开始）
    /// </summary>
    [Tooltip("目标平台索引（从0开始）")]
    public int targetPlatformIndex = 1;

    /// <summary>
    /// 是否已生成过NPC - 确保只生成一次
    /// </summary>
    private bool hasSpawned = false;

    /// <summary>
    /// 在平台上生成NPC - 由PlatformSpawner调用
    /// </summary>
    /// <param name="platform">平台对象</param>
    /// <param name="platformIndex">平台索引</param>
    public void SpawnNPCOnPlatform(GameObject platform, int platformIndex)
    {
        // 检查是否符合生成条件：
        // 1. 平台索引必须等于目标索引
        // 2. 还没有生成过NPC
        // 3. NPC预制体已设置
        if (platformIndex != targetPlatformIndex || hasSpawned || npcPrefab == null)
            return;

        // 获取平台的Renderer组件
        var renderer = platform.GetComponent<Renderer>();
        if (renderer == null) return;

        // 获取平台的碰撞边界
        var bounds = renderer.bounds;

        // 计算NPC生成位置（平台中心上方）
        float x = bounds.center.x;
        float z = bounds.center.z;
        float y = bounds.max.y + npcHeightOffset;

        Vector3 spawnPosition = new Vector3(x, y, z);

        // 直接创建NPC（不使用对象池）
        GameObject npc = Instantiate(npcPrefab, spawnPosition, Quaternion.identity);
        
        // 将NPC设置为平台的子对象，这样平台消失时NPC也会跟着消失
        npc.transform.parent = platform.transform;

        // 标记已生成
        hasSpawned = true;

        // 获取平台的Platform组件，标记有NPC
        Platform platformComp = platform.GetComponent<Platform>();
        if (platformComp != null)
        {
            platformComp.HasNPC = true;
        }

        Debug.Log($"SimpleNPCSpawner: 在平台 #{platformIndex} 生成NPC");
    }
}