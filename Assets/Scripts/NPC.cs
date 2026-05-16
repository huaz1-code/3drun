using UnityEngine;

/// <summary>
/// NPC脚本 - 控制单个NPC的行为
/// 功能说明：
/// 1. NPC左右巡逻移动
/// 2. 在设定的范围内来回移动
/// 3. 碰到边界后反向移动
/// </summary>
public class NPC : MonoBehaviour
{
    /// <summary>
    /// 移动速度 - NPC每秒移动的距离（单位/秒）
    /// 值越大，移动越快
    /// </summary>
    [Tooltip("移动速度")]
    public float moveSpeed = 2f;

    /// <summary>
    /// 移动范围 - NPC左右移动的边界距离
    /// NPC会在起始点左右各移动这个距离的范围
    /// </summary>
    [Tooltip("移动范围（左右）")]
    public float moveRange = 3f;

    /// <summary>
    /// 起始位置 - NPC生成时的位置
    /// 作为移动范围的中心点
    /// </summary>
    private Vector3 startPosition;

    /// <summary>
    /// 移动方向 - 1表示向右，-1表示向左
    /// 碰到边界时反转
    /// </summary>
    private float direction = 1f;

    /// <summary>
    /// 初始化方法 - 对象激活时调用一次
    /// 职责：记录起始位置
    /// </summary>
    void Start()
    {
        // 记录当前位置作为起始点
        startPosition = transform.position;
    }

    /// <summary>
    /// 更新方法 - 每帧调用
    /// 职责：处理NPC的巡逻移动
    /// </summary>
    void Update()
    {
        // ========== 计算新位置 ==========
        // 在X轴方向移动
        // direction控制方向（+1或-1）
        // moveSpeed控制速度
        // Time.deltaTime保证移动与帧率无关
        float newX = transform.position.x + direction * moveSpeed * Time.deltaTime;

        // ========== 边界检测 ==========
        // 检查是否到达左右边界
        // 如果新位置超出范围，反转方向
        if (newX >= startPosition.x + moveRange || newX <= startPosition.x - moveRange)
        {
            // 反转移动方向
            direction *= -1;
        }

        // ========== 更新位置 ==========
        // 使用Clamp限制位置在范围内
        // 确保NPC不会超出设定的移动边界
        transform.position = new Vector3(
            Mathf.Clamp(newX, startPosition.x - moveRange, startPosition.x + moveRange),
            transform.position.y,  // 保持Y和Z坐标不变
            transform.position.z
        );
    }

    /// <summary>
    /// 重置NPC - 重用时调用的初始化方法
    /// 在对象池获取NPC时调用
    /// </summary>
    public void ResetNPC()
    {
        // 记录新的起始位置为当前位置
        startPosition = transform.position;

        // 随机设置初始移动方向
        // Random.Range(0, 2) 返回0或1
        // 如果是0，direction设为1；如果是1，direction设为-1
        direction = Random.Range(0, 2) == 0 ? 1f : -1f;
    }
}
