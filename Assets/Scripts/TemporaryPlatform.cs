using UnityEngine;

/// <summary>
/// 临时平台脚本 - 玩家站在上面一段时间后平台消失
/// 功能说明：
/// 1. 使用向上重叠检测（OverlapBox）判断玩家是否站在平台上
/// 2. 检测到玩家后开始计时，计时不可被玩家离开中断
/// 3. 计时达到设定时间后，先禁用碰撞体让玩家掉落，再隐藏渲染器
/// </summary>
public class TemporaryPlatform : MonoBehaviour
{
    /// <summary>
    /// 消失延迟时间 - 玩家站在平台上多久后平台消失（秒）
    /// 默认1秒
    /// </summary>
    [Tooltip("玩家站在平台上后消失的延迟时间（秒）")]
    public float disappearDelay = 1f;

    /// <summary>
    /// 检测高度 - 从平台顶部向上检测的范围（米）
    /// 用于确定玩家是否站在平台上
    /// </summary>
    [Tooltip("从平台顶部向上检测的高度范围（米）")]
    public float detectionHeight = 1f;

    /// <summary>
    /// 是否已经触发计时（一旦触发不会重置）
    /// </summary>
    private bool hasTriggered = false;

    /// <summary>
    /// 是否正在计时中
    /// </summary>
    private bool isTiming = false;

    /// <summary>
    /// 当前计时时间
    /// </summary>
    private float currentTime = 0f;

    /// <summary>
    /// 平台的碰撞体组件引用
    /// </summary>
    private Collider platformCollider;

    /// <summary>
    /// 平台的渲染器组件引用
    /// </summary>
    private MeshRenderer platformRenderer;

    /// <summary>
    /// 初始化方法 - 游戏开始时调用一次
    /// 职责：获取碰撞体和渲染器组件引用
    /// </summary>
    void Start()
    {
        // 获取碰撞体组件
        platformCollider = GetComponent<Collider>();
        if (platformCollider == null)
        {
            Debug.LogError("TemporaryPlatform: 没有找到 Collider 组件！");
        }

        // 获取渲染器组件
        platformRenderer = GetComponent<MeshRenderer>();
        if (platformRenderer == null)
        {
            Debug.LogError("TemporaryPlatform: 没有找到 MeshRenderer 组件！");
        }
    }

    /// <summary>
    /// 物理更新方法 - 按固定时间间隔调用
    /// 职责：使用向上重叠检测判断玩家是否站在平台上
    /// </summary>
    void FixedUpdate()
    {
        // 如果已经触发计时或者已经消失，不再检测
        if (hasTriggered || !platformCollider.enabled)
        {
            return;
        }

        // 检测玩家是否站在平台上
        if (IsPlayerOnPlatform())
        {
            // 开始计时（一旦触发不会重置）
            hasTriggered = true;
            isTiming = true;
            currentTime = 0f;
        }
    }

    /// <summary>
    /// 更新方法 - 每帧调用一次
    /// 职责：处理计时逻辑
    /// </summary>
    void Update()
    {
        // 如果正在计时中，累加时间
        if (isTiming)
        {
            currentTime += Time.deltaTime;

            // 检查是否达到消失延迟时间
            if (currentTime >= disappearDelay)
            {
                Disappear();
            }
        }
    }

    /// <summary>
    /// 检测玩家是否站在平台上
    /// 使用 OverlapBox 从平台顶部向上检测
    /// </summary>
    /// <returns>如果玩家站在平台上返回 true，否则返回 false</returns>
    bool IsPlayerOnPlatform()
    {
        if (platformCollider == null)
        {
            return false;
        }

        // 获取平台碰撞体的边界
        Bounds bounds = platformCollider.bounds;

        // 计算检测盒子的中心点（在平台顶部上方）
        Vector3 center = new Vector3(
            bounds.center.x,
            bounds.max.y + detectionHeight / 2f,
            bounds.center.z
        );

        // 计算检测盒子的半尺寸
        // X 和 Z 与平台相同，Y 为检测高度的一半
        Vector3 halfExtents = new Vector3(
            bounds.extents.x,
            detectionHeight / 2f,
            bounds.extents.z
        );

        // 使用 OverlapBox 检测所有层的碰撞体，然后通过 Tag 过滤
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, Quaternion.identity);

        // 检查是否有玩家碰撞体（排除平台自己）
        foreach (Collider col in hitColliders)
        {
            // 跳过平台自己的碰撞体
            if (col == platformCollider || col.transform.IsChildOf(transform))
            {
                continue;
            }

            // 检查是否是玩家
            if (col.gameObject.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 平台消失方法
    /// 职责：先禁用碰撞体让玩家掉落，再隐藏渲染器
    /// </summary>
    void Disappear()
    {
        // 停止计时
        isTiming = false;

        // 先禁用碰撞体，让玩家可以掉落
        if (platformCollider != null)
        {
            platformCollider.enabled = false;
        }

        // 然后隐藏渲染器
        if (platformRenderer != null)
        {
            platformRenderer.enabled = false;
        }
    }

    /// <summary>
    /// 重置平台状态 - 用于对象池复用或重新激活平台
    /// 职责：恢复碰撞体和渲染器，重置计时器和触发状态
    /// </summary>
    public void ResetPlatform()
    {
        // 恢复碰撞体
        if (platformCollider != null)
        {
            platformCollider.enabled = true;
        }

        // 恢复渲染器
        if (platformRenderer != null)
        {
            platformRenderer.enabled = true;
        }

        // 重置计时和触发状态
        hasTriggered = false;
        isTiming = false;
        currentTime = 0f;
    }

    /// <summary>
    /// Gizmos 绘制方法 - 用于在 Scene 视图中显示检测区域
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (platformCollider == null)
        {
            return;
        }

        Bounds bounds = platformCollider.bounds;

        // 检测区域中心点
        Vector3 center = new Vector3(
            bounds.center.x,
            bounds.max.y + detectionHeight / 2f,
            bounds.center.z
        );

        // 检测区域半尺寸
        Vector3 halfExtents = new Vector3(
            bounds.extents.x,
            detectionHeight / 2f,
            bounds.extents.z
        );

        // 设置 Gizmos 颜色为半透明绿色
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);

        // 绘制检测区域盒子
        Gizmos.DrawCube(center, halfExtents * 2f);

        // 设置 Gizmos 颜色为绿色边框
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, halfExtents * 2f);
    }
}