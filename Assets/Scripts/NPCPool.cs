using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NPC对象池脚本 - 管理NPC的重复利用，避免频繁的Instantiate和Destroy
/// 功能说明：
/// 1. 单例模式，确保全局只有一个实例
/// 2. 预创建NPC对象池
/// 3. 提供获取和归还NPC的方法
/// 4. 支持动态扩展池大小
/// </summary>
public class NPCPool : MonoBehaviour
{
    /// <summary>
    /// 单例实例 - 全局唯一的对象池引用
    /// 用于其他脚本访问NPC池
    /// </summary>
    public static NPCPool Instance;

    /// <summary>
    /// NPC预制体 - 用于创建NPC实例的模板
    /// 必须是包含NPC组件的预制体
    /// </summary>
    [Tooltip("NPC预制体")]
    public GameObject npcPrefab;

    /// <summary>
    /// 初始池子大小 - 游戏开始时预创建的NPC数量
    /// 预先创建可以避免游戏开始时的卡顿
    /// </summary>
    [Tooltip("初始池子大小")]
    public int initialPoolSize = 5;

    /// <summary>
    /// 最大池子大小 - 允许的最大NPC数量
    /// 防止无限创建导致性能问题
    /// </summary>
    [Tooltip("最大池子大小")]
    public int maxPoolSize = 15;

    /// <summary>
    /// NPC对象池 - 使用队列存储可用的NPC对象
    /// 队列的FIFO特性适合对象池的获取和归还
    /// </summary>
    private Queue<GameObject> npcPool = new Queue<GameObject>();

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
        }
        else
        {
            // 已经有实例存在，销毁当前对象
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初始化方法 - 游戏开始时调用一次
    /// 职责：开始初始化对象池
    /// </summary>
    void Start()
    {
        InitializePool();
    }

    /// <summary>
    /// 初始化对象池 - 预创建指定数量的NPC实例
    /// 在游戏开始时调用，避免运行时创建造成卡顿
    /// </summary>
    void InitializePool()
    {
        // 循环创建指定数量的NPC
        for (int i = 0; i < initialPoolSize; i++)
        {
            // 使用预制体创建NPC
            GameObject npc = Instantiate(npcPrefab, transform);

            // 初始设为非激活状态
            npc.SetActive(false);

            // 加入对象池队列
            npcPool.Enqueue(npc);
        }
    }

    /// <summary>
    /// 获取NPC - 从对象池获取一个可用的NPC
    /// 如果池为空且未达到最大数量，则动态创建
    /// </summary>
    /// <param name="position">NPC的放置位置（世界坐标）</param>
    /// <returns>可用的NPC对象，如果池已满则返回null</returns>
    public GameObject GetNPC(Vector3 position)
    {
        GameObject npc = null;

        // 优先从池中获取
        if (npcPool.Count > 0)
        {
            // 从队列头部取出一个NPC
            npc = npcPool.Dequeue();
        }
        // 池为空但未达到最大数量，动态创建
        else if (transform.childCount < maxPoolSize)
        {
            // 动态创建NPC
            npc = Instantiate(npcPrefab, transform);
        }

        if (npc != null)
        {
            // 激活NPC
            npc.SetActive(true);

            // 设置位置
            npc.transform.position = position;

            // 调用NPC的ResetNPC方法
            NPC npcComp = npc.GetComponent<NPC>();
            if (npcComp != null)
            {
                npcComp.ResetNPC();
            }
        }

        return npc;
    }

    /// <summary>
    /// 归还NPC - 将NPC归还到对象池
    /// NPC会被设为非激活状态并加入队列
    /// </summary>
    /// <param name="npc">要归还的NPC对象</param>
    public void ReturnNPC(GameObject npc)
    {
        // 检查是否有效
        if (npc == null) return;

        // 设为非激活状态
        npc.SetActive(false);

        // 加入对象池队列
        npcPool.Enqueue(npc);
    }
}
