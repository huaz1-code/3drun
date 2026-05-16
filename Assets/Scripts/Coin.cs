using UnityEngine;

/// <summary>
/// 金币脚本 - 控制单个金币的行为
/// 功能说明：
/// 1. 金币旋转动画
/// 2. 金币上下浮动效果
/// 3. 玩家碰撞检测
/// 4. 被收集后归还到对象池
/// </summary>
public class Coin : MonoBehaviour
{
    /// <summary>
    /// 旋转速度 - 金币每秒旋转的角度（度/秒）
    /// 正值表示绕Y轴逆时针旋转
    /// </summary>
    [Tooltip("旋转速度")]
    public float rotationSpeed = 180f;

    /// <summary>
    /// 浮动高度 - 金币上下浮动的幅度（单位）
    /// 值为0.3表示金币会在原始位置上下浮动0.3个单位
    /// </summary>
    [Tooltip("浮动高度")]
    public float floatHeight = 0.3f;

    /// <summary>
    /// 浮动速度 - 金币浮动的频率系数
    /// 值越大，浮动越快
    /// 与Mathf.Sin配合实现周期性浮动
    /// </summary>
    [Tooltip("浮动速度")]
    public float floatSpeed = 2f;

    /// <summary>
    /// 原始位置 - 金币生成时的初始位置
    /// 用于计算浮动偏移量
    /// </summary>
    private Vector3 originalPosition;

    /// <summary>
    /// 时间偏移 - 用于错开多个金币的浮动相位
    /// 避免所有金币同时上下浮动，看起来更自然
    /// 值范围：0到2π
    /// </summary>
    private float timeOffset;

    /// <summary>
    /// 所属平台引用 - 记录金币所在的平台
    /// 用于收集时通知平台清除引用
    /// </summary>
    private Platform parentPlatform;

    /// <summary>
    /// 初始化方法 - 对象激活时调用一次
    /// 职责：记录初始位置，随机化浮动相位
    /// </summary>
    void Start()
    {
        // 记录当前的初始位置
        // 如果金币被移动，这个值会更新
        originalPosition = transform.position;

        // 随机生成时间偏移
        // Random.Range 返回[0, 2π)范围内的随机值
        // 乘以2π确保在完整的正弦周期内随机
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    /// <summary>
    /// 更新方法 - 每帧调用
    /// 职责：处理金币旋转和浮动动画
    /// </summary>
    void Update()
    {
        // ========== 金币旋转 ==========
        // 绕Y轴旋转（世界坐标）
        // Vector3.up 表示绕Y轴旋转
        // 旋转角度 = 速度 * 时间
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // ========== 金币浮动 ==========
        // 使用正弦函数计算高度偏移
        // Sin函数的特性：
        // - 值域：[-1, 1]
        // - 周期：2π
        // - 平滑连续
        // Time.time * floatSpeed：将游戏时间转换为弧度
        // + timeOffset：添加随机相位偏移
        float offsetY = Mathf.Sin(Time.time * floatSpeed + timeOffset) * floatHeight;

        // 计算浮动后的位置
        // 原始位置 + 浮动偏移 = 新位置
        transform.position = originalPosition + new Vector3(0, offsetY, 0);
    }

    /// <summary>
    /// 碰撞进入回调 - 当其他碰撞器进入触发器时调用
    /// 职责：检测玩家碰撞，收集金币
    /// </summary>
    /// <param name="other">进入的碰撞器</param>
    void OnTriggerEnter(Collider other)
    {
        // 检查碰撞对象是否带有"Player"标签
        // CompareTag 比 == 操作符更高效
        if (other.CompareTag("Player"))
        {
            // 增加金币计数
            CoinCounter.AddCoin();

            // 通知所属平台，金币已被收集
            if (parentPlatform != null)
            {
                // 通知平台清除金币引用
                parentPlatform.SetCoin(null);
            }

            // 将金币归还到对象池
            if (CoinPool.Instance != null)
            {
                // 对象池负责隐藏和复用金币
                CoinPool.Instance.ReturnCoin(gameObject);
            }
            else
            {
                // 如果没有对象池，直接销毁
                // 这是后备方案，不应该发生
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// 重置金币 - 初始化或重用金币时的状态重置
    /// 在对象池获取金币时调用
    /// </summary>
    public void ResetCoin()
    {
        // 更新原始位置为当前位置
        originalPosition = transform.position;

        // 重新随机化浮动相位
        // 确保重用的金币有不同的浮动节奏
        timeOffset = Random.Range(0f, Mathf.PI * 2f);

        // 清空所属平台引用
        parentPlatform = null;
    }

    /// <summary>
    /// 设置所属平台 - 记录金币所在的平台对象
    /// </summary>
    /// <param name="platform">金币所属的平台</param>
    public void SetParentPlatform(Platform platform)
    {
        parentPlatform = platform;
    }
}
