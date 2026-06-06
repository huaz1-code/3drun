using UnityEngine;

/// <summary>
/// 平台生成器脚本 - 负责无限平台地图的生成和管理
/// 功能说明：
/// 1. 根据玩家位置动态生成新平台
/// 2. 回收玩家身后的旧平台
/// 3. 在新平台上生成金币
/// 4. 围绕圆柱体螺旋上升生成平台
/// </summary>
public class PlatformSpawner : MonoBehaviour
{
    /// <summary>
    /// 玩家引用 - 用于获取玩家当前位置
    /// 生成平台时以玩家位置为基准
    /// </summary>
    [Tooltip("玩家引用")]
    public Transform player;

    /// <summary>
    /// 圆柱体中心点 - 平台围绕此点螺旋上升
    /// </summary>
    [Tooltip("圆柱体中心点")]
    public Transform cylinderCenter;

    /// <summary>
    /// 前方保持的平台数量 - 玩家前方始终保留的平台数量
    /// 值越大，视野范围越远，但性能消耗越高
    /// </summary>
    [Tooltip("前方保持的平台数量")]
    public int platformCountAhead = 8;

    /// <summary>
    /// 螺旋半径 - 平台围绕圆柱体的距离
    /// </summary>
    [Tooltip("螺旋半径")]
    public float spiralRadius = 6f;

    /// <summary>
    /// 每平台上升高度 - 相邻平台之间的垂直高度差
    /// </summary>
    [Tooltip("每平台上升高度")]
    public float heightPerPlatform = 1.5f;

    /// <summary>
    /// 每平台旋转角度 - 相邻平台之间的旋转角度（度）
    /// 360度为一圈
    /// </summary>
    [Tooltip("每平台旋转角度（度）")]
    public float rotationPerPlatform = 45f;

    /// <summary>
    /// 平台最小宽度 - X轴最小缩放值
    /// </summary>
    [Tooltip("平台最小宽度")]
    public float minScaleX = 3f;

    /// <summary>
    /// 平台最大宽度 - X轴最大缩放值
    /// </summary>
    [Tooltip("平台最大宽度")]
    public float maxScaleX = 7f;

    /// <summary>
    /// 平台高度 - Y轴缩放值
    /// 决定平台的厚度（保持不变）
    /// </summary>
    [Tooltip("平台高度")]
    public float scaleY = 0.5f;

    /// <summary>
    /// 平台最小深度 - Z轴最小缩放值
    /// </summary>
    [Tooltip("平台最小深度")]
    public float minScaleZ = 2f;

    /// <summary>
    /// 平台最大深度 - Z轴最大缩放值
    /// </summary>
    [Tooltip("平台最大深度")]
    public float maxScaleZ = 4f;

    [Tooltip("伤害平台生成概率（0-1）")]
    [Range(0f, 1f)]
    public float damagePlatformChance = 0.1f;

    [Tooltip("临时平台生成概率（0-1）")]
    [Range(0f, 1f)]
    public float temporaryPlatformChance = 0.1f;

    /// <summary>
    /// 回收距离 - 玩家身后超过此距离的平台将被回收
    /// 值越大，旧平台保留时间越长
    /// </summary>
    [Tooltip("回收距离（玩家后方）")]
    public float recycleDistance = 15f;

    /// <summary>
    /// 调试模式 - 是否输出生成和回收的调试信息
    /// </summary>
    [Tooltip("调试模式")]
    public bool debugMode = true;

    /// <summary>
    /// 平台计数器 - 记录已生成的平台总数
    /// 用于螺旋位置计算
    /// </summary>
    private int platformIndex = 0;

    /// <summary>
    /// 初始化完成标记
    /// Start方法执行完毕后设为true
    /// </summary>
    private bool initialized = false;

    /// <summary>
    /// 初始化方法 - 游戏开始时调用一次
    /// 职责：查找玩家引用，检查依赖，生成初始平台
    /// </summary>
    void Start()
    {
        if (debugMode) Debug.Log("PlatformSpawner: 启动");

        // 查找玩家对象
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                if (debugMode) Debug.Log("PlatformSpawner: 找到玩家 " + player.name);
            }
            else
            {
                if (debugMode) Debug.LogError("PlatformSpawner: 未找到玩家对象！请确保场景中有名为'player'的对象");
                enabled = false;
                return;
            }
        }

        // 检查平台池是否存在
        if (PlatformPool.Instance == null)
        {
            if (debugMode) Debug.LogError("PlatformSpawner: 未找到PlatformPool！请确保场景中有PlatformPool对象");
            enabled = false;
            return;
        }

        // 如果没有设置圆柱体中心点，使用默认位置
        if (cylinderCenter == null)
        {
            if (debugMode) Debug.LogWarning("PlatformSpawner: 未设置圆柱体中心点，使用原点");
        }

        // 预生成前方所需的平台
        for (int i = 0; i < platformCountAhead; i++)
        {
            SpawnPlatform();
        }

        // 标记初始化完成
        initialized = true;

        if (debugMode)
            Debug.Log($"PlatformSpawner: 初始化完成，生成{platformCountAhead}个平台");
    }

    /// <summary>
    /// 更新方法 - 每帧调用
    /// 职责：检测玩家位置，决定是否生成新平台或回收旧平台
    /// </summary>
    void Update()
    {
        if (!initialized || player == null) return;

        // 计算玩家相对于圆柱体中心的高度
        float playerRelativeHeight = player.position.y;
        
        // 计算最高可见平台的索引（基于玩家高度）
        int targetPlatformIndex = Mathf.FloorToInt(playerRelativeHeight / heightPerPlatform) + platformCountAhead + 1;

        // 如果需要生成新平台
        while (platformIndex < targetPlatformIndex)
        {
            SpawnPlatform();
        }

        // 回收玩家下方的旧平台
        RecycleOldPlatforms(playerRelativeHeight);
    }

    /// <summary>
    /// 生成平台 - 创建单个新平台
    /// 使用螺旋公式计算平台位置
    /// </summary>
    void SpawnPlatform()
    {
        if (PlatformPool.Instance == null)
        {
            if (debugMode) Debug.LogError("SpawnPlatform: PlatformPool.Instance为null");
            return;
        }

        // 获取圆柱体中心位置（默认使用原点）
        Vector3 centerPos = cylinderCenter != null ? cylinderCenter.position : Vector3.zero;

        // 计算当前平台的角度（转换为弧度）
        float angle = Mathf.Deg2Rad * platformIndex * rotationPerPlatform;

        // 使用螺旋公式计算平台位置
        // x = centerX + radius * cos(angle)
        // z = centerZ + radius * sin(angle)
        // y = baseY + heightPerPlatform * index
        float x = centerPos.x + spiralRadius * Mathf.Cos(angle);
        float z = centerPos.z + spiralRadius * Mathf.Sin(angle);
        float y = centerPos.y + heightPerPlatform * platformIndex;

        // 构建平台的位置和缩放向量
        Vector3 position = new Vector3(x, y, z);
        // 随机生成平台宽度和深度，保持厚度不变
        float randomScaleX = Random.Range(minScaleX, maxScaleX);
        float randomScaleZ = Random.Range(minScaleZ, maxScaleZ);
        Vector3 scale = new Vector3(randomScaleX, scaleY, randomScaleZ);

        // 从对象池获取平台实例
        GameObject platform = PlatformPool.Instance.GetPlatform(position, scale);

        if (platform != null)
            {
                // 随机选择平台类型
                PlatformType platformType = GetRandomPlatformType();

                // 设置平台类型
                Platform platformComp = platform.GetComponent<Platform>();
                if (platformComp != null)
                {
                    platformComp.SetPlatformType(platformType);
                }

                if (debugMode)
                    Debug.Log($"SpawnPlatform: 生成平台 #{platformIndex} ({platformType}) at ({x:F2}, {y:F2}, {z:F2})");

                // 生成金币（不在特殊平台上生成）
                if (platformType == PlatformType.Normal)
                {
                    CoinSpawner coinSpawner = FindObjectOfType<CoinSpawner>();
                    if (coinSpawner != null)
                    {
                        coinSpawner.SpawnCoinsOnNewPlatform(platform);
                    }
                }

                // 平台计数器递增
                platformIndex++;
            }
        else
        {
            if (debugMode)
                Debug.LogWarning("SpawnPlatform: 未能获取平台，池可能已满");
        }
    }

    /// <summary>
    /// 回收旧平台 - 将玩家下方的平台归还到对象池
    /// </summary>
    /// <param name="playerY">当前玩家Y位置</param>
    void RecycleOldPlatforms(float playerY)
    {
        if (PlatformPool.Instance == null) return;

        // 倒序遍历子对象（避免删除元素时索引错误）
        for (int i = PlatformPool.Instance.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = PlatformPool.Instance.transform.GetChild(i);

            // 只处理激活状态的对象
            if (child.gameObject.activeSelf)
            {
                // 检查是否在玩家下方超过回收距离
                if (child.position.y < playerY - recycleDistance)
                {
                    if (debugMode)
                        Debug.Log($"RecycleOldPlatforms: 回收平台 at Y={child.position.y}");

                    // 将平台归还到对象池
                    PlatformPool.Instance.ReturnPlatform(child.gameObject);
                }
            }
        }
    }

    PlatformType GetRandomPlatformType()
    {
        float random = Random.value;

        if (random < damagePlatformChance)
        {
            return PlatformType.Damage;
        }
        random -= damagePlatformChance;

        if (random < temporaryPlatformChance)
        {
            return PlatformType.Temporary;
        }

        return PlatformType.Normal;
    }
}