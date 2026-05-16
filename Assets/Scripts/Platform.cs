using UnityEngine;

/// <summary>
/// 平台脚本 - 负责管理单个平台的状态
/// 功能说明：
/// 1. 记录当前平台上的金币和Enemy引用
/// 2. 提供金币/Enemy的设置和回收接口
/// 3. 随机切换平台材质（视觉效果）
/// 4. 对象回收时自动清理子对象
/// </summary>
public class Platform : MonoBehaviour
{
    /// <summary>
    /// 材质列表 - 平台可以随机切换的材质数组
    /// 在Inspector中拖拽多个材质，平台会随机选择
    /// 用于增加视觉多样性
    /// </summary>
    [Tooltip("材质列表（随机切换）")]
    public Material[] materials;

    /// <summary>
    /// 调试模式 - 是否输出调试日志
    /// 开启后会在金币/Enemy设置和回收时打印信息
    /// </summary>
    [Tooltip("调试模式")]
    public bool debugMode = false;

    /// <summary>
    /// 平台渲染器引用 - 用于更换材质
    /// </summary>
    private Renderer platformRenderer;

    /// <summary>
    /// 当前金币引用 - 记录此平台上生成的金币
    /// null表示没有金币
    /// </summary>
    private GameObject currentCoin;

    /// <summary>
    /// 当前Enemy引用 - 记录此平台上生成的Enemy
    /// null表示没有Enemy
    /// </summary>
    private GameObject currentEnemy;

    /// <summary>
    /// Enemy存在标记 - 表示平台上是否有Enemy
    /// 用于避免在有Enemy的平台上生成金币
    /// </summary>
    public bool HasEnemy { get; set; }

    /// <summary>
    /// 唤醒方法 - 对象创建时调用（在Start之前）
    /// 职责：获取渲染器组件引用
    /// </summary>
    void Awake()
    {
        // 获取挂载此脚本的GameObject上的Renderer组件
        // Renderer用于访问和修改材质
        platformRenderer = GetComponent<Renderer>();
    }

    /// <summary>
    /// 重置平台 - 回收金币/Enemy并随机切换材质
    /// 在对象从池中取出重用时调用
    /// 确保平台每次使用都是干净的状态
    /// </summary>
    public void ResetPlatform()
    {
        // 调试日志
        if (debugMode) Debug.Log($"Platform.ResetPlatform: {name}, 金币: {currentCoin}, Enemy: {currentEnemy}");

        // 回收当前金币（如果有）
        RecycleCoin();

        // 回收当前Enemy（如果有）
        RecycleEnemy();

        // 重置Enemy标记
        HasEnemy = false;

        // 随机切换材质
        RandomizeMaterial();
    }

    /// <summary>
    /// 随机切换材质 - 从材质列表中随机选择一个应用
    /// 如果没有设置材质或列表为空，则不执行
    /// </summary>
    void RandomizeMaterial()
    {
        // 检查渲染器和材质列表是否有效
        if (platformRenderer != null && materials != null && materials.Length > 0)
        {
            // 生成随机索引
            // Random.Range(min, max) 返回[min, max)范围内的整数
            // 对于整数参数，max是不包含的
            int randomIndex = Random.Range(0, materials.Length);

            // 应用随机材质
            platformRenderer.material = materials[randomIndex];
        }
    }

    /// <summary>
    /// 检查是否有金币 - 判断平台上当前是否持有金币
    /// </summary>
    /// <returns>true表示有金币，false表示没有</returns>
    public bool HasCoin()
    {
        // currentCoin为null时返回false
        return currentCoin != null;
    }

    /// <summary>
    /// 设置金币 - 在平台上放置金币
    /// </summary>
    /// <param name="coin">要设置的金币GameObject</param>
    public void SetCoin(GameObject coin)
    {
        // 调试日志
        if (debugMode && coin != null)
            Debug.Log($"Platform.SetCoin: {name} 设置金币 {coin.name}");

        // 记录金币引用
        currentCoin = coin;
    }

    /// <summary>
    /// 设置Enemy - 在平台上放置Enemy
    /// </summary>
    /// <param name="enemy">要设置的Enemy GameObject</param>
    public void SetEnemy(GameObject enemy)
    {
        // 调试日志
        if (debugMode && enemy != null)
            Debug.Log($"Platform.SetEnemy: {name} 设置Enemy {enemy.name}");

        // 记录Enemy引用
        currentEnemy = enemy;
    }

    /// <summary>
    /// 回收金币 - 将金币归还到对象池
    /// 如果没有金币则跳过
    /// </summary>
    public void RecycleCoin()
    {
        // 检查是否有金币需要回收
        if (currentCoin != null)
        {
            // 调试日志
            if (debugMode)
                Debug.Log($"Platform.RecycleCoin: {name} 回收金币 {currentCoin.name}");

            // 检查金币池是否存在
            if (CoinPool.Instance != null)
            {
                // 调用金币池的回收方法
                // 金币池负责将金币设为非激活状态并放回队列
                CoinPool.Instance.ReturnCoin(currentCoin);
            }

            // 清空引用
            currentCoin = null;
        }
        else if (debugMode)
        {
            // 没有金币时的调试日志
            Debug.Log($"Platform.RecycleCoin: {name} 没有金币可回收");
        }
    }

    /// <summary>
    /// 回收Enemy - 将Enemy归还到对象池
    /// 如果没有Enemy则跳过
    /// </summary>
    public void RecycleEnemy()
    {
        // 检查是否有Enemy需要回收
        if (currentEnemy != null)
        {
            // 调试日志
            if (debugMode)
                Debug.Log($"Platform.RecycleEnemy: {name} 回收Enemy {currentEnemy.name}");

            // 检查Enemy池是否存在
            if (EnemyPool.Instance != null)
            {
                // 调用Enemy池的回收方法
                // Enemy池负责将Enemy设为非激活状态并放回队列
                EnemyPool.Instance.ReturnEnemy(currentEnemy);
            }

            // 清空引用
            currentEnemy = null;
        }

        // 重置Enemy标记
        HasEnemy = false;
    }

    /// <summary>
    /// 销毁回调 - 当平台被销毁时调用
    /// 确保平台销毁时金币和Enemy也被正确回收
    /// 防止内存泄漏
    /// </summary>
    void OnDestroy()
    {
        // 调用回收方法，确保子对象被正确清理
        RecycleCoin();
        RecycleEnemy();
    }
}
