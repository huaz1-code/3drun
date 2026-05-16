using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 平台对象池脚本 - 管理平台的重复利用，避免频繁的Instantiate和Destroy
/// 功能说明：
/// 1. 单例模式，确保全局只有一个实例
/// 2. 预创建平台对象池
/// 3. 提供获取和归还平台的方法
/// 4. 支持动态扩展池大小
/// </summary>
public class PlatformPool : MonoBehaviour
{
    /// <summary>
    /// 单例实例 - 全局唯一的对象池引用
    /// 用于其他脚本访问平台池
    /// </summary>
    public static PlatformPool Instance;

    /// <summary>
    /// 平台预制体 - 用于创建平台实例的模板
    /// 必须是包含Platform组件的预制体
    /// </summary>
    [Tooltip("平台预制体")]
    public GameObject platformPrefab;

    /// <summary>
    /// 初始池子大小 - 游戏开始时预创建的平台数量
    /// 预先创建可以避免游戏开始时的卡顿
    /// </summary>
    [Tooltip("初始池子大小")]
    public int initialPoolSize = 15;

    /// <summary>
    /// 最大池子大小 - 允许的最大平台数量
    /// 防止无限创建导致性能问题
    /// </summary>
    [Tooltip("最大池子大小")]
    public int maxPoolSize = 30;

    /// <summary>
    /// 调试模式 - 是否输出调试日志
    /// </summary>
    [Tooltip("调试模式")]
    public bool debugMode = true;

    /// <summary>
    /// 平台对象池 - 使用队列存储可用的平台对象
    /// 队列的FIFO特性适合对象池的获取和归还
    /// </summary>
    private Queue<GameObject> platformPool = new Queue<GameObject>();

    /// <summary>
    /// 唤醒方法 - 对象创建时调用（在Start之前）
    /// 职责：实现单例模式
    /// </summary>
    void Awake()
    {
        // 检查是否已存在实例
        if (Instance == null)
        {
            // 第一次创建，设为全局实例
            Instance = this;
            if (debugMode) Debug.Log("PlatformPool: 单例创建成功");
        }
        else if (Instance != this)
        {
            // 已经有实例存在，说明有重复，销毁当前对象
            if (debugMode) Debug.LogWarning("PlatformPool: 已存在实例，销毁重复对象");
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// 初始化方法 - 游戏开始时调用一次
    /// 职责：检查预制体，开始初始化对象池
    /// </summary>
    void Start()
    {
        // 检查是否设置了平台预制体
        if (platformPrefab == null)
        {
            if (debugMode)
                Debug.LogError("PlatformPool: platformPrefab未设置！请在Inspector中指定平台预制体");
            enabled = false;
            return;
        }

        // 开始初始化
        InitializePool();
    }

    /// <summary>
    /// 初始化对象池 - 预创建指定数量的平台实例
    /// 在游戏开始时调用，避免运行时创建造成卡顿
    /// </summary>
    void InitializePool()
    {
        if (debugMode)
            Debug.Log($"PlatformPool: 开始初始化 {initialPoolSize} 个平台");

        // 循环创建指定数量的平台
        for (int i = 0; i < initialPoolSize; i++)
        {
            // 使用预制体创建平台
            GameObject platform = Instantiate(platformPrefab, transform);

            // 设置平台名称，便于调试
            platform.name = "Platform_" + (i + 1);

            // 初始设为非激活状态（等待需要时激活）
            platform.SetActive(false);

            // 加入对象池队列
            platformPool.Enqueue(platform);
        }

        if (debugMode)
            Debug.Log($"PlatformPool: 初始化完成，池中共有 {platformPool.Count} 个平台");
    }

    /// <summary>
    /// 获取平台 - 从对象池获取一个可用的平台
    /// 如果池为空且未达到最大数量，则动态创建
    /// </summary>
    /// <param name="position">平台的放置位置（世界坐标）</param>
    /// <param name="scale">平台的缩放大小</param>
    /// <returns>可用的平台对象，如果池已满则返回null</returns>
    public GameObject GetPlatform(Vector3 position, Vector3 scale)
    {
        GameObject platform = null;

        // 优先从池中获取
        if (platformPool.Count > 0)
        {
            // 从队列头部取出一个平台
            platform = platformPool.Dequeue();
        }
        // 池为空但未达到最大数量，动态创建
        else if (transform.childCount < maxPoolSize)
        {
            if (debugMode) Debug.LogWarning("PlatformPool: 池已空，动态创建新平台");
            platform = Instantiate(platformPrefab, transform);
            platform.name = "Platform_" + (transform.childCount);
        }
        // 达到最大数量，无法获取
        else
        {
            if (debugMode) Debug.LogError("PlatformPool: 达到最大数量，无法获取平台");
            return null;
        }

        if (platform != null)
        {
            // 激活平台
            platform.SetActive(true);

            // 设置位置和大小
            platform.transform.position = position;
            platform.transform.localScale = scale;

            // 调用平台的ResetPlatform方法，清理之前的金币和NPC
            Platform platformComp = platform.GetComponent<Platform>();
            if (platformComp != null)
            {
                platformComp.ResetPlatform();
            }
        }

        return platform;
    }

    /// <summary>
    /// 归还平台 - 将平台归还到对象池
    /// 平台会被设为非激活状态并加入队列
    /// </summary>
    /// <param name="platform">要归还的平台对象</param>
    public void ReturnPlatform(GameObject platform)
    {
        // 检查是否有效
        if (platform == null) return;

        // 调用平台的ResetPlatform方法
        Platform platformComp = platform.GetComponent<Platform>();
        if (platformComp != null)
        {
            platformComp.ResetPlatform();
        }

        // 设为非激活状态
        platform.SetActive(false);

        // 加入对象池队列
        platformPool.Enqueue(platform);
    }
}
