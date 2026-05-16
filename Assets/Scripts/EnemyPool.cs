using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Enemy对象池脚本 - 管理Enemy的重复利用，避免频繁的Instantiate和Destroy
/// 功能说明：
/// 1. 单例模式，确保全局只有一个实例
/// 2. 预创建Enemy对象池
/// 3. 提供获取和归还Enemy的方法
/// 4. 支持动态扩展池大小
/// </summary>
public class EnemyPool : MonoBehaviour
{
    /// <summary>
    /// 单例实例 - 全局唯一的对象池引用
    /// 用于其他脚本访问Enemy池
    /// </summary>
    public static EnemyPool Instance;

    /// <summary>
    /// Enemy预制体 - 用于创建Enemy实例的模板
    /// 必须是包含Enemy组件的预制体
    /// </summary>
    [Tooltip("Enemy预制体")]
    public GameObject enemyPrefab;

    /// <summary>
    /// 初始池子大小 - 游戏开始时预创建的Enemy数量
    /// 预先创建可以避免游戏开始时的卡顿
    /// </summary>
    [Tooltip("初始池子大小")]
    public int initialPoolSize = 5;

    /// <summary>
    /// 最大池子大小 - 允许的最大Enemy数量
    /// 防止无限创建导致性能问题
    /// </summary>
    [Tooltip("最大池子大小")]
    public int maxPoolSize = 15;

    /// <summary>
    /// Enemy对象池 - 使用队列存储可用的Enemy对象
    /// 队列的FIFO特性适合对象池的获取和归还
    /// </summary>
    private Queue<GameObject> enemyPool = new Queue<GameObject>();

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
    /// 初始化对象池 - 预创建指定数量的Enemy实例
    /// 在游戏开始时调用，避免运行时创建造成卡顿
    /// </summary>
    void InitializePool()
    {
        // 循环创建指定数量的Enemy
        for (int i = 0; i < initialPoolSize; i++)
        {
            // 使用预制体创建Enemy
            GameObject enemy = Instantiate(enemyPrefab, transform);

            // 初始设为非激活状态
            enemy.SetActive(false);

            // 加入对象池队列
            enemyPool.Enqueue(enemy);
        }
    }

    /// <summary>
    /// 获取Enemy - 从对象池获取一个可用的Enemy
    /// 如果池为空且未达到最大数量，则动态创建
    /// </summary>
    /// <param name="position">Enemy的放置位置（世界坐标）</param>
    /// <returns>可用的Enemy对象，如果池已满则返回null</returns>
    public GameObject GetEnemy(Vector3 position)
    {
        GameObject enemy = null;

        // 优先从池中获取
        if (enemyPool.Count > 0)
        {
            // 从队列头部取出一个Enemy
            enemy = enemyPool.Dequeue();
        }
        // 池为空但未达到最大数量，动态创建
        else if (transform.childCount < maxPoolSize)
        {
            // 动态创建Enemy
            enemy = Instantiate(enemyPrefab, transform);
        }

        if (enemy != null)
        {
            // 激活Enemy
            enemy.SetActive(true);

            // 设置位置
            enemy.transform.position = position;

            // 调用Enemy的ResetEnemy方法
            Enemy enemyComp = enemy.GetComponent<Enemy>();
            if (enemyComp != null)
            {
                enemyComp.ResetEnemy();
            }
        }

        return enemy;
    }

    /// <summary>
    /// 归还Enemy - 将Enemy归还到对象池
    /// Enemy会被设为非激活状态并加入队列
    /// </summary>
    /// <param name="enemy">要归还的Enemy对象</param>
    public void ReturnEnemy(GameObject enemy)
    {
        // 检查是否有效
        if (enemy == null) return;

        // 设为非激活状态
        enemy.SetActive(false);

        // 加入对象池队列
        enemyPool.Enqueue(enemy);
    }
}
