using UnityEngine;

/// <summary>
/// Enemy生成器脚本 - 负责在平台上生成Enemy
/// 功能说明：
/// 1. 根据平台索引决定是否生成Enemy
/// 2. 在符合条件的平台上生成Enemy
/// 3. 设置Enemy与平台的关联
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    /// <summary>
    /// Enemy高度偏移 - Enemy在平台上方的垂直距离
    /// 正值表示在平台上方
    /// </summary>
    [Tooltip("Enemy在平台上方的偏移")]
    public float enemyHeightOffset = 1.5f;

    /// <summary>
    /// Enemy生成间隔 - 每隔几个平台生成一个Enemy
    /// 值为10表示每10个平台生成1个Enemy
    /// </summary>
    [Tooltip("Enemy生成间隔（每几个平台生成一个）")]
    public int spawnInterval = 10;

    /// <summary>
    /// Enemy开始生成的平台索引 - 前几个平台不生成Enemy
    /// 用于给玩家留出初始安全区域
    /// </summary>
    [Tooltip("Enemy开始生成的平台索引")]
    public int startSpawnIndex = 10;

    /// <summary>
    /// 在平台上生成Enemy - 根据平台索引决定是否生成
    /// 由PlatformSpawner在生成新平台后调用
    /// </summary>
    /// <param name="platform">要生成Enemy的平台</param>
    /// <param name="platformIndex">平台的索引编号</param>
    public void SpawnEnemyOnPlatform(GameObject platform, int platformIndex)
    {
        // ========== 检查是否符合生成条件 ==========
        // 条件1：平台索引必须大于起始索引
        // 条件2：平台索引必须是生成间隔的倍数
        if (platformIndex < startSpawnIndex || platformIndex % spawnInterval != 0)
            return;

        // 检查Enemy池是否可用
        if (EnemyPool.Instance == null) return;

        // 获取平台的Renderer组件（用于获取边界）
        var renderer = platform.GetComponent<Renderer>();
        if (renderer == null) return;

        // 获取平台的碰撞边界
        var bounds = renderer.bounds;

        // ========== 计算Enemy生成位置 ==========
        // 在平台范围内随机选择X和Z坐标
        // 留出边缘空白区域（1f）
        float randomX = Random.Range(bounds.min.x + 1f, bounds.max.x - 1f);
        float randomZ = Random.Range(bounds.min.z + 1f, bounds.max.z - 1f);

        // Enemy位置在平台上方
        float y = bounds.max.y + enemyHeightOffset;

        // 构建生成位置
        Vector3 spawnPosition = new Vector3(randomX, y, randomZ);

        // ========== 从对象池获取Enemy ==========
        GameObject enemy = EnemyPool.Instance.GetEnemy(spawnPosition);

        if (enemy != null)
        {
            // 获取平台的Platform组件
            Platform platformComp = platform.GetComponent<Platform>();
            if (platformComp != null)
            {
                // 标记平台有Enemy
                platformComp.HasEnemy = true;

                // 设置Enemy引用
                platformComp.SetEnemy(enemy);
            }
        }
    }
}
