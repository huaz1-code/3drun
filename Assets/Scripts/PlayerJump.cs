using UnityEngine;

/// <summary>
/// 玩家跳跃脚本 - 负责处理玩家的跳跃和重力系统
/// 功能说明：
/// 1. 空格键触发跳跃
/// 2. 使用OverlapBox进行地面检测
/// 3. 手动重力系统（关闭Rigidbody默认重力）
/// 4. 空中速度控制
/// </summary>
public class PlayerJump : MonoBehaviour
{
    /// <summary>
    /// 跳跃初始速度 - 跳跃时给予的垂直向上的初速度
    /// 值越大，跳得越高
    /// 与重力设置共同决定跳跃高度：h = v^2 / (2g)
    /// </summary>
    [Tooltip("跳跃初始速度")]
    public float jumpVelocity = 8f;

    /// <summary>
    /// 重力加速度 - 模拟地球引力的加速值
    /// 值越大，下落越快
    /// 建议值：20-30（接近真实感的下落速度）
    /// </summary>
    [Tooltip("重力加速度")]
    public float gravity = 20f;

    /// <summary>
    /// 地面检测盒子半尺寸 - OverlapBox检测的盒子大小
    /// 使用一半尺寸（halfExtents）定义盒子
    /// X、Z值应略小于角色宽度，确保只在脚底检测
    /// Y值应较小，用于检测正下方的平台
    /// </summary>
    [Tooltip("地面检测盒子半宽")]
    public Vector3 groundCheckHalfExtents = new Vector3(0.45f, 0.05f, 0.45f);

    /// <summary>
    /// 地面检测偏移 - 检测盒子相对于角色中心的位置
    /// Y值为负数表示向下偏移（在角色脚下）
    /// 偏移量应略小于角色高度的一半
    /// </summary>
    [Tooltip("地面检测盒子中心点偏移")]
    public Vector3 groundCheckOffset = new Vector3(0, -0.5f, 0);

    /// <summary>
    /// 地面层 - 用于地面检测的物理层
    /// 只有标记为此Layer的对象才会被地面检测命中
    /// 建议：创建专用的"Ground"或"Platform"层
    /// </summary>
    [Tooltip("地面层")]
    public LayerMask groundLayer;

    /// <summary>
    /// 调试模式 - 是否输出地面检测调试信息
    /// 开启后会在控制台打印检测结果和跳跃状态
    /// </summary>
    [Tooltip("是否打印调试信息")]
    public bool debugMode = true;

    /// <summary>
    /// 玩家刚体引用 - 用于设置速度和施加重力
    /// </summary>
    private Rigidbody rb;

    /// <summary>
    /// 地面状态标记 - 表示玩家是否站在地面上
    /// true = 在地面上，可以跳跃
    /// false = 在空中，不能跳跃
    /// </summary>
    private bool isGrounded = false;

    /// <summary>
    /// 是否已经销毁初始Plane
    /// 用于确保Plane只被销毁一次
    /// </summary>
    private bool hasDestroyedPlane = false;

    /// <summary>
    /// 初始化方法 - 游戏开始时调用一次
    /// 职责：获取刚体，关闭默认重力，检查配置
    /// </summary>
    void Start()
    {
        // 获取挂载此脚本的GameObject上的Rigidbody组件
        rb = GetComponent<Rigidbody>();

        // 检查是否成功获取Rigidbody
        if (rb == null)
        {
            // 如果没有Rigidbody，输出错误日志并禁用脚本
            Debug.LogError("PlayerJump: 没有找到Rigidbody组件！");
            enabled = false;
            return;
        }

        // 关闭Rigidbody的默认重力
        // 原因：我们使用手动重力，可以更精确地控制下落行为
        // 注意：关闭后如果忘记手动施加重力，角色会飘在空中
        rb.useGravity = false;

        // 检查是否设置了地面层
        // LayerMask.value == 0 表示没有选择任何层
        if (groundLayer.value == 0)
        {
            Debug.LogWarning("PlayerJump: 请在Inspector中设置Ground Layer！");
        }
    }

    /// <summary>
    /// 更新方法 - 每帧调用一次
    /// 职责：处理跳跃输入检测
    /// </summary>
    void Update()
    {
        // 在Update中处理输入，因为输入检测与帧率相关
        HandleJumpInput();
    }

    /// <summary>
    /// 物理更新方法 - 按固定时间间隔调用
    /// 职责：地面检测和应用重力
    /// </summary>
    void FixedUpdate()
    {
        // 检测玩家是否在地面上
        // 这是跳跃系统的基础，决定是否可以跳跃
        isGrounded = IsOnFloor();

        // 应用重力效果
        // 重力应该在FixedUpdate中应用，保证物理一致性
        ApplyGravity();

        // 调试信息输出
        if (debugMode)
        {
            // F2 表示保留2位小数
            Debug.Log($"地面检测: {isGrounded}, 速度Y: {rb.velocity.y:F2}");
        }
    }

    /// <summary>
    /// 地面检测 - 使用OverlapBox进行碰撞检测
    /// 原理：在角色脚下创建一个虚拟盒子，检测是否有物体进入
    /// 优点：比Raycast更可靠，不受高速移动影响
    /// </summary>
    /// <returns>true表示在地面上，false表示在空中</returns>
    bool IsOnFloor()
    {
        // 如果没有设置地面层，直接返回false
        // 防止后续Physics.OverlapBox出错
        if (groundLayer.value == 0)
            return false;

        // 计算检测盒子的世界坐标
        // 角色位置 + 偏移量 = 检测盒子中心点
        Vector3 center = transform.position + groundCheckOffset;

        // 使用OverlapBox检测碰撞
        // 参数说明：
        // center: 检测盒子中心点（世界坐标）
        // halfExtents: 盒子半尺寸（各轴向的一半）
        // orientation: 盒子旋转（使用角色旋转）
        // layerMask: 检测哪些层
        // 返回值：所有命中物体的碰撞器数组
        Collider[] hitColliders = Physics.OverlapBox(
            center,                     // 检测盒子中心
            groundCheckHalfExtents,     // 盒子半尺寸
            transform.rotation,         // 盒子旋转（与角色一致）
            groundLayer                 // 只检测地面层
        );

        // 检查是否检测到地面
        bool foundGround = hitColliders.Length > 0;

        // 如果检测到地面，且还没有销毁初始Plane，检查是否跳到了生成的平台上
        if (foundGround && !hasDestroyedPlane)
        {
            CheckAndDestroyInitialPlane(hitColliders);
        }

        // 如果命中任何碰撞器，说明在地面上
        // Length > 0 表示有物体被检测到
        return foundGround;
    }

    /// <summary>
    /// 检查并销毁初始Plane
    /// 当玩家第一次跳到生成的平台（Platform_开头）上时，销毁Plane
    /// </summary>
    /// <param name="hitColliders">检测到的碰撞器数组</param>
    void CheckAndDestroyInitialPlane(Collider[] hitColliders)
    {
        // 遍历所有检测到的碰撞器
        foreach (var collider in hitColliders)
        {
            GameObject hitObject = collider.gameObject;

            // 检查是否跳到了生成的平台上（名称以"Platform_"开头）
            if (hitObject.name.StartsWith("Platform_"))
            {
                // 查找并销毁初始Plane对象
                GameObject planeObj = GameObject.Find("Plane");
                if (planeObj != null)
                {
                    Destroy(planeObj);
                    hasDestroyedPlane = true;
                    Debug.Log("PlayerJump: 玩家第一次跳到平台上，销毁初始Plane");
                }
                break; // 只需要销毁一次
            }
        }
    }

    /// <summary>
    /// 处理跳跃输入 - 检测空格键按下
    /// 职责：判断是否可以跳跃，执行跳跃
    /// </summary>
    void HandleJumpInput()
    {
        // 检测空格键按下（每帧只触发一次）
        // GetButtonDown 在按键按下的那一帧返回true
        // GetButton 会持续返回true直到释放
        if (Input.GetButtonDown("Jump"))
        {
            // 检查是否在地面上
            if (isGrounded)
            {
                // 执行跳跃
                // 保持X和Z速度不变，只设置Y速度
                // 这样跳跃时不会打断水平移动
                rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);

                if (debugMode)
                    Debug.Log("跳跃！");
            }
            else if (debugMode)
            {
                Debug.Log("不在地面上，无法跳跃");
            }
        }
    }

    /// <summary>
    /// 应用重力 - 手动施加重力加速度
    /// 原理：每帧给刚体一个向下的速度增量
    /// 公式：v = v0 + g * dt
    /// </summary>
    void ApplyGravity()
    {
        // 如果不在地面上，应用重力
        if (!isGrounded)
        {
            // 向下的速度增量 = 重力加速度 * 时间间隔
            // Time.fixedDeltaTime 是物理更新的固定时间步长
            // Vector3.down 等价于 new Vector3(0, -1, 0)
            rb.velocity += Vector3.down * gravity * Time.fixedDeltaTime;
        }
        // 如果在地面上且正在下落（速度为负）
        else if (rb.velocity.y < 0)
        {
            // 将Y速度设为0，停止下落
            // 这是为了防止速度累积导致的穿透问题
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        }
    }

    /// <summary>
    /// 绘制调试Gizmos - 在Scene视图中显示地面检测区域
    /// 仅在选中此GameObject时显示
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 设置Gizmo颜色：地面上为绿色，空中为红色
        Gizmos.color = isGrounded ? Color.green : Color.red;

        // 计算检测盒子中心点的世界坐标
        Vector3 center = transform.position + groundCheckOffset;

        // 绘制线框盒子
        // 使用2倍半尺寸得到完整尺寸
        // DrawWireCube绘制线框立方体
        Gizmos.DrawWireCube(center, groundCheckHalfExtents * 2);
    }
}
