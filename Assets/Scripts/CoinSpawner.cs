using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 金币生成器脚本 - 负责在平台上生成金币
/// 功能说明：
/// 1. 为已存在的平台生成金币
/// 2. 为新生成的平台生成金币
/// 3. 根据概率随机决定是否生成
/// 4. 避免在有Enemy的平台上生成金币
/// </summary>
public class CoinSpawner : MonoBehaviour
{
    /// <summary>
    /// 金币生成概率 - 每个平台生成金币的可能性（0到1）
    /// 0表示不生成，1表示每个平台都生成
    /// 0.6表示60%的平台会有金币
    /// </summary>
    [Tooltip("每个平台生成金币的概率（0-1）")]
    public float coinSpawnChance = 0.6f;

    /// <summary>
    /// 金币高度偏移 - 金币在平台上方的垂直距离
    /// 正值表示在平台上方
    /// </summary>
    [Tooltip("金币在平台上方的偏移")]
    public float coinHeightOffset = 1f;

    /// <summary>
    /// 边缘留空 - 金币生成位置距平台边缘的距离
    /// 防止金币生成在平台边缘，玩家难以收集
    /// </summary>
    [Tooltip("金币生成的边缘留空")]
    public float edgePadding = 0.5f;

    /// <summary>
    /// 初始化方法 - 游戏开始时调用一次
    /// 职责：为场景中已存在的平台生成金币
    /// </summary>
    void Start()
    {
        // 为已存在的平台生成金币
        SpawnCoinsOnExistingPlatforms();
    }

    /// <summary>
    /// 为已存在平台生成金币 - 初始化时调用
    /// 遍历场景中所有平台，为符合条件的生成金币
    /// </summary>
    void SpawnCoinsOnExistingPlatforms()
    {
        // 获取所有平台对象
        var platforms = FindAllPlatforms();

        // 遍历每个平台
        foreach (var platform in platforms)
        {
            // 获取平台的Platform组件
            Platform platformComp = platform.GetComponent<Platform>();

            // 检查是否符合生成条件：
            // 1. 平台必须有Platform组件
            // 2. 平台当前没有金币
            // 3. 平台上没有Enemy
            if (platformComp != null && !platformComp.HasCoin() && !platformComp.HasEnemy)
            {
                // 使用概率决定是否生成
                // Random.value 返回[0, 1)范围内的随机值
                if (Random.value < coinSpawnChance)
                {
                    SpawnSingleCoinOnPlatform(platform);
                }
            }
        }
    }

    /// <summary>
    /// 在新平台上生成金币 - 平台生成时调用
    /// 由PlatformSpawner在生成新平台后调用
    /// </summary>
    /// <param name="platform">要生成金币的平台</param>
    public void SpawnCoinsOnNewPlatform(GameObject platform)
    {
        // 检查平台是否有效
        if (platform == null) return;

        // 获取平台的Platform组件
        Platform platformComp = platform.GetComponent<Platform>();

        // 检查是否符合生成条件
        if (platformComp == null || platformComp.HasCoin() || platformComp.HasEnemy)
        {
            return;
        }

        // 使用概率决定是否生成
        if (Random.value < coinSpawnChance)
        {
            SpawnSingleCoinOnPlatform(platform);
        }
    }

    /// <summary>
    /// 查找所有平台 - 遍历场景获取平台列表
    /// 查找名称以"Platform_"开头的激活对象
    /// </summary>
    /// <returns>所有平台对象的列表</returns>
    List<GameObject> FindAllPlatforms()
    {
        // 获取场景中所有GameObject
        var allObjects = FindObjectsOfType<GameObject>();

        // 创建平台列表
        var platforms = new List<GameObject>();

        // 遍历所有对象
        foreach (var obj in allObjects)
        {
            // 检查对象名称是否符合平台命名规范
            // 检查对象是否处于激活状态
            if (obj.name.StartsWith("Platform_") && obj.activeSelf)
            {
                platforms.Add(obj);
            }
        }

        return platforms;
    }

    /// <summary>
    /// 在单个平台上生成金币 - 实际生成金币的方法
    /// 在平台表面随机位置生成一个金币
    /// </summary>
    /// <param name="platform">金币所属的平台</param>
    void SpawnSingleCoinOnPlatform(GameObject platform)
    {
        // 获取平台的Platform组件
        Platform platformComp = platform.GetComponent<Platform>();
        if (platformComp == null) return;

        // 再次检查平台是否已有金币
        if (platformComp.HasCoin())
        {
            Debug.LogWarning($"CoinSpawner: 平台 {platform.name} 已有金币，跳过");
            return;
        }

        // 获取平台的Renderer组件（用于获取边界）
        var renderer = platform.GetComponent<Renderer>();
        if (renderer == null) return;

        // 获取平台的碰撞边界
        var bounds = renderer.bounds;

        // 在平台范围内随机选择生成位置
        // 留出边缘空白区域
        float randomX = Random.Range(bounds.min.x + edgePadding, bounds.max.x - edgePadding);
        float randomZ = Random.Range(bounds.min.z + edgePadding, bounds.max.z - edgePadding);

        // 金币位置在平台上方
        float randomY = bounds.max.y + coinHeightOffset;

        // 构建生成位置
        Vector3 spawnPosition = new Vector3(randomX, randomY, randomZ);

        // 从金币池获取金币
        if (CoinPool.Instance != null)
        {
            // 获取金币对象
            GameObject coin = CoinPool.Instance.GetCoin(spawnPosition);

            if (coin != null)
            {
                // 通知平台，金币已设置
                platformComp.SetCoin(coin);

                // 获取金币的Coin组件，设置所属平台
                Coin coinComp = coin.GetComponent<Coin>();
                if (coinComp != null)
                {
                    coinComp.SetParentPlatform(platformComp);
                }
            }
        }
    }
}
