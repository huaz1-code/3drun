using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 金币对象池脚本 - 管理金币的重复利用，避免频繁的Instantiate和Destroy
/// 功能说明：
/// 1. 单例模式，确保全局只有一个实例
/// 2. 预创建金币对象池
/// 3. 提供获取和归还金币的方法
/// 4. 支持动态扩展池大小
/// </summary>
public class CoinPool : MonoBehaviour
{
    /// <summary>
    /// 单例实例 - 全局唯一的对象池引用
    /// 用于其他脚本访问金币池
    /// </summary>
    public static CoinPool Instance;

    /// <summary>
    /// 金币预制体 - 用于创建金币实例的模板
    /// 必须是包含Coin组件的预制体
    /// </summary>
    public GameObject coinPrefab;

    /// <summary>
    /// 初始池子大小 - 游戏开始时预创建的金币数量
    /// 预先创建可以避免游戏开始时的卡顿
    /// </summary>
    public int initialPoolSize = 20;

    /// <summary>
    /// 最大池子大小 - 允许的最大金币数量
    /// 防止无限创建导致性能问题
    /// </summary>
    public int maxPoolSize = 50;

    /// <summary>
    /// 金币对象池 - 使用队列存储可用的金币对象
    /// 队列的FIFO特性适合对象池的获取和归还
    /// </summary>
    private Queue<GameObject> coinPool = new Queue<GameObject>();

    /// <summary>
    /// 唤醒方法 - 对象创建时调用（在Start之前）
    /// 职责：实现单例模式，开始初始化
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

        // 开始初始化
        InitializePool();
    }

    /// <summary>
    /// 初始化对象池 - 预创建指定数量的金币实例
    /// 在游戏开始时调用，避免运行时创建造成卡顿
    /// </summary>
    void InitializePool()
    {
        // 循环创建指定数量的金币
        for (int i = 0; i < initialPoolSize; i++)
        {
            // 使用预制体创建金币
            GameObject coin = Instantiate(coinPrefab);

            // 初始设为非激活状态
            coin.SetActive(false);

            // 设置父对象为当前对象池
            coin.transform.SetParent(transform);

            // 加入对象池队列
            coinPool.Enqueue(coin);
        }
    }

    /// <summary>
    /// 获取金币 - 从对象池获取一个可用的金币
    /// 如果池为空且未达到最大数量，则动态创建
    /// </summary>
    /// <param name="position">金币的放置位置（世界坐标）</param>
    /// <returns>可用的金币对象，如果池已满则返回null</returns>
    public GameObject GetCoin(Vector3 position)
    {
        // 优先从池中获取
        if (coinPool.Count > 0)
        {
            // 从队列头部取出一个金币
            GameObject coin = coinPool.Dequeue();

            // 激活金币
            coin.SetActive(true);

            // 设置位置
            coin.transform.position = position;

            // 调用金币的ResetCoin方法
            Coin coinComp = coin.GetComponent<Coin>();
            if (coinComp != null)
            {
                coinComp.ResetCoin();
            }

            return coin;
        }
        // 池为空但未达到最大数量，动态创建
        else if (transform.childCount < maxPoolSize)
        {
            // 动态创建金币
            GameObject coin = Instantiate(coinPrefab);

            // 设置父对象
            coin.transform.SetParent(transform);

            // 设置位置
            coin.transform.position = position;

            return coin;
        }

        // 达到最大数量，无法获取
        return null;
    }

    /// <summary>
    /// 归还金币 - 将金币归还到对象池
    /// 金币会被设为非激活状态并加入队列
    /// </summary>
    /// <param name="coin">要归还的金币对象</param>
    public void ReturnCoin(GameObject coin)
    {
        // 设为非激活状态
        coin.SetActive(false);

        // 加入对象池队列
        coinPool.Enqueue(coin);
    }
}
