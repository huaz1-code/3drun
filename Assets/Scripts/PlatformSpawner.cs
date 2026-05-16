using UnityEngine;

/// <summary>
/// 平台生成器脚本 - 负责无限平台地图的生成和管理
/// 功能说明：
/// 1. 根据玩家位置动态生成新平台
/// 2. 回收玩家身后的旧平台
/// 3. 在新平台上生成Enemy和金币
/// 4. 实现无限跑酷地图的错觉
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
    /// 前方保持的平台数量 - 玩家前方始终保留的平台数量
    /// 值越大，视野范围越远，但性能消耗越高
    /// </summary>
    [Tooltip("前方保持的平台数量")]
    public int platformCountAhead = 8;

    /// <summary>
    /// 平台X坐标最小值 - 平台在X轴的随机范围下限
    /// 控制平台左右分布的最小位置
    /// </summary>
    [Tooltip("平台最小X位置")]
    public float minX = -5f;

    /// <summary>
    /// 平台X坐标最大值 - 平台在X轴的随机范围上限
    /// 控制平台左右分布的最大位置
    /// </summary>
    [Tooltip("平台最大X位置")]
    public float maxX = 5f;

    /// <summary>
    /// 平台Y坐标最小值 - 平台在Y轴的随机范围下限
    /// 控制平台上下高度变化的最小值
    /// </summary>
    [Tooltip("平台最小Y位置")]
    public float minY = 0f;

    /// <summary>
    /// 平台Y坐标最大值 - 平台在Y轴的随机范围上限
    /// 控制平台上下高度变化的最大值
    /// </summary>
    [Tooltip("平台最大Y位置")]
    public float maxY = 4f;

    /// <summary>
    /// 平台Z轴间距 - 相邻平台在Z轴的间隔距离
    /// 值越大，平台越稀疏；值越小，平台越密集
    /// </summary>
    [Tooltip("平台Z轴间距")]
    public float zSpacing = 8f;

    /// <summary>
    /// 平台宽度 - X轴缩放值
    /// 决定平台的左右长度
    /// </summary>
    [Tooltip("平台宽度")]
    public float scaleX = 5f;

    /// <summary>
    /// 平台高度 - Y轴缩放值
    /// 决定平台的厚度
    /// </summary>
    [Tooltip("平台高度")]
    public float scaleY = 0.5f;

    /// <summary>
    /// 平台深度 - Z轴缩放值
    /// 决定平台的前后长度
    /// </summary>
    [Tooltip("平台深度")]
    public float scaleZ = 3f;

    /// <summary>
    /// 回收距离 - 玩家身后超过此距离的平台将被回收
    /// 值越大，旧平台保留时间越长
    /// </summary>
    [Tooltip("回收距离（玩家后方）")]
    public float recycleDistance = 15f;

    /// <summary>
    /// 第一个平台偏移 - 第一个平台相对于玩家位置的Z轴偏移
    /// 正值表示在玩家前方，负值表示在玩家后方
    /// </summary>
    [Tooltip("第一个平台距离玩家的偏移（Z轴）")]
    public float firstPlatformOffset = 5f;

    /// <summary>
    /// 调试模式 - 是否输出生成和回收的调试信息
    /// </summary>
    [Tooltip("调试模式")]
    public bool debugMode = true;

    /// <summary>
    /// 下一个平台生成位置（Z轴）
    /// 记录下一个平台应该生成的Z坐标
    /// </summary>
    private float nextSpawnZ;

    /// <summary>
    /// 上次记录的玩家Z位置
    /// 用于检测玩家是否在前进
    /// </summary>
    private float lastPlayerZ;

    /// <summary>
    /// 初始化完成标记
    /// Start方法执行完毕后设为true
    /// </summary>
    private bool initialized = false;

    /// <summary>
    /// 平台计数器 - 记录已生成的平台总数
    /// 用于Enemy生成的间隔判断
    /// </summary>
    private int platformIndex = 0;

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
            // 尝试通过名称查找玩家
            GameObject playerObj = GameObject.Find("player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                if (debugMode) Debug.Log("PlatformSpawner: 找到玩家 " + player.name);
            }
            else
            {
                // 找不到玩家，输出错误并禁用脚本
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

        // 初始化上次玩家位置为当前玩家位置
        lastPlayerZ = player.position.z;

        // 设置第一个平台的生成位置
        // 玩家当前位置 + 偏移量 = 第一个平台的位置
        nextSpawnZ = player.position.z + firstPlatformOffset;

        if (debugMode)
            Debug.Log($"PlatformSpawner: 玩家位置 Z={lastPlayerZ}，第一个平台从 Z={nextSpawnZ} 开始");

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
        // 如果未初始化或玩家引用为空，直接返回
        if (!initialized || player == null) return;

        // 获取当前玩家Z位置
        float playerZ = player.position.z;

        // 只有玩家前进时才生成新平台（防止后退时生成）
        if (playerZ > lastPlayerZ)
        {
            // 生成足够前方区域的平台
            // 循环直到前方有足够的平台
            while (nextSpawnZ < playerZ + platformCountAhead * zSpacing)
            {
                SpawnPlatform();
            }

            // 回收玩家身后的旧平台
            RecycleOldPlatforms(playerZ);
        }

        // 更新上次玩家位置
        lastPlayerZ = playerZ;
    }

    /// <summary>
    /// 生成平台 - 创建单个新平台
    /// 从对象池获取平台，设置位置和大小，然后生成Enemy和金币
    /// </summary>
    void SpawnPlatform()
    {
        // 检查平台池是否可用
        if (PlatformPool.Instance == null)
        {
            if (debugMode) Debug.LogError("SpawnPlatform: PlatformPool.Instance为null");
            return;
        }

        // 在指定范围内随机生成位置
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        // 构建平台的位置和缩放向量
        Vector3 position = new Vector3(randomX, randomY, nextSpawnZ);
        Vector3 scale = new Vector3(scaleX, scaleY, scaleZ);

        // 从对象池获取平台实例
        GameObject platform = PlatformPool.Instance.GetPlatform(position, scale);

        if (platform != null)
        {
            if (debugMode)
                Debug.Log($"SpawnPlatform: 生成平台 #{platformIndex} at Z={nextSpawnZ}");

            // 查找Enemy生成器，在新平台上生成Enemy
            EnemySpawner enemySpawner = FindObjectOfType<EnemySpawner>();
            if (enemySpawner != null)
            {
                // 传入平台和索引，生成器会决定是否生成Enemy
                enemySpawner.SpawnEnemyOnPlatform(platform, platformIndex);
            }

            // 查找金币生成器，在新平台上生成金币
            CoinSpawner coinSpawner = FindObjectOfType<CoinSpawner>();
            if (coinSpawner != null)
            {
                coinSpawner.SpawnCoinsOnNewPlatform(platform);
            }

            // 平台计数器递增
            platformIndex++;
        }
        else
        {
            if (debugMode)
                Debug.LogWarning("SpawnPlatform: 未能获取平台，池可能已满");
        }

        // 更新下一个平台的生成位置
        nextSpawnZ += zSpacing;
    }

    /// <summary>
    /// 回收旧平台 - 将玩家身后的平台归还到对象池
    /// 通过遍历平台池的子对象实现
    /// </summary>
    /// <param name="playerZ">当前玩家Z位置</param>
    void RecycleOldPlatforms(float playerZ)
    {
        // 检查平台池是否可用
        if (PlatformPool.Instance == null) return;

        // 倒序遍历子对象（避免删除元素时索引错误）
        for (int i = PlatformPool.Instance.transform.childCount - 1; i >= 0; i--)
        {
            // 获取子对象
            Transform child = PlatformPool.Instance.transform.GetChild(i);

            // 只处理激活状态的对象
            if (child.gameObject.activeSelf)
            {
                // 检查是否在玩家身后超过回收距离
                // 如果平台的Z坐标 < 玩家Z坐标 - 回收距离，说明平台已在身后
                if (child.position.z < playerZ - recycleDistance)
                {
                    if (debugMode)
                        Debug.Log($"RecycleOldPlatforms: 回收平台 at Z={child.position.z}");

                    // 将平台归还到对象池
                    PlatformPool.Instance.ReturnPlatform(child.gameObject);
                }
            }
        }
    }
}
