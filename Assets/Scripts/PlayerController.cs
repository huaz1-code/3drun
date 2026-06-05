using UnityEngine;
using System.Collections;

/// <summary>
/// 玩家控制器脚本 - 负责处理玩家的移动和旋转
/// 功能说明：
/// 1. 基于WASD/方向键的相对摄像机方向的移动
/// 2. 指数曲线加速（输入时快速加速）
/// 3. 对数曲线减速（停止输入时平滑减速）
/// 4. 角色朝向跟随移动方向平滑旋转
/// </summary>
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// 最大移动速度 - 玩家能达到的最大水平移动速度（单位/秒）
    /// 控制玩家奔跑的极限速度
    /// </summary>
    [Tooltip("最大移动速度")]
    public float moveSpeed = 9f;

    /// <summary>
    /// 伤害值 - 受到伤害时的扣血量
    /// </summary>
    [Tooltip("伤害值")]
    public float damageOnHit = 10f;

    /// <summary>
    /// 受伤冷却时间 - 两次受伤之间的间隔（秒）
    /// 防止连续碰撞造成多次伤害
    /// </summary>
    [Tooltip("受伤冷却时间（秒）")]
    public float damageCooldown = 1f;

    /// <summary>
    /// 玩家血量组件引用
    /// </summary>
    private Health health;

    /// <summary>
    /// 加速因子 - 控制加速曲线的陡峭程度
    /// 值越大，加速越快，曲线越陡峭
    /// 使用指数函数 Math.Exp 实现非线性加速
    /// </summary>
    [Tooltip("加速因子（指数曲线）")]
    public float accelerationFactor = 2f;

    /// <summary>
    /// 减速因子 - 控制减速曲线的平滑程度
    /// 值越大，减速越急促；值越小，滑行距离越长
    /// 使用对数函数 Math.Log 实现平滑减速
    /// </summary>
    [Tooltip("减速因子（对数曲线）")]
    public float decelerationFactor = 0.5f;

    /// <summary>
    /// 摄像机引用 - 用于计算相对移动方向
    /// 玩家移动方向基于摄像机朝向
    /// </summary>
    [Tooltip("摄像机引用")]
    public Transform cameraTransform;

    /// <summary>
    /// 玩家刚体组件引用 - 用于物理移动
    /// Rigidbody负责实际的物理模拟
    /// </summary>
    private Rigidbody rb;

    /// <summary>
    /// 初始化方法 - 游戏开始时调用一次
    /// 职责：获取刚体组件，自动查找主摄像机，获取血量组件
    /// </summary>
    void Start()
    {
        // 获取挂载此脚本的GameObject上的Rigidbody组件
        // Rigidbody用于物理模拟和速度控制
        rb = GetComponent<Rigidbody>();

        // 检查是否在Inspector中手动指定了摄像机
        if (cameraTransform == null)
        {
            // 自动查找场景中的主摄像机（Tag为MainCamera的对象）
            // Camera.main是一个开销较小的缓存调用
            if (Camera.main != null)
            {
                // 获取主摄像机的Transform组件用于方向计算
                cameraTransform = Camera.main.transform;
            }
        }

        // 获取玩家的血量组件
        health = GetComponent<Health>();
        if (health == null)
        {
            Debug.LogWarning("PlayerController: 未找到 Health 组件，请确保玩家对象上挂载了 Health.cs");
        }
    }

    /// <summary>
    /// 更新方法 - 每帧调用一次（约60fps）
    /// 职责：处理玩家旋转逻辑（纯视觉，不涉及物理）
    /// </summary>
    void Update()
    {
        // 如果游戏暂停，不处理输入
        if (Time.timeScale <= 0f) return;

        // 处理玩家角色朝向旋转
        // 旋转在Update中处理，因为涉及平滑插值，与帧率相关
        HandleRotation();
    }

    /// <summary>
    /// 物理更新方法 - 按固定时间间隔调用（默认0.02秒）
    /// 职责：处理玩家移动物理逻辑
    /// 注意：物理计算必须在FixedUpdate中进行，保证一致性
    /// </summary>
    void FixedUpdate()
    {
        // 如果游戏暂停，不处理输入
        if (Time.timeScale <= 0f) return;

        // 处理玩家移动物理
        // 使用固定时间步长确保物理模拟的稳定性
        HandleMovement();
    }

    /// <summary>
    /// 获取移动方向 - 根据输入和摄像机朝向计算实际移动方向
    /// 核心算法：将摄像机朝向投影到水平面，与输入向量结合
    /// </summary>
    /// <returns>归一化的世界空间移动方向向量</returns>
    Vector3 GetMovementDirection()
    {
        // 获取原始输入值，范围[-1, 1]
        // Horizontal对应A/D或左/右方向键
        // Vertical对应W/S或上/下方向键
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        // 将摄像机的前方向投影到水平面（忽略垂直分量）
        // Vector3.up表示世界空间的Y轴向上方向
        // ProjectOnPlane会在指定平面上投影向量
        // 这样无论摄像机如何俯仰，移动始终在地面上进行
        Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;

        // 同理计算摄像机右方向在水平面的投影
        // right叉乘forward得到垂直向右的方向
        Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;

        // 组合水平和垂直输入，返回最终移动方向
        // forward * moveVertical 控制前后移动
        // right * moveHorizontal 控制左右移动
        // 两者相加得到综合移动方向
        return forward * moveVertical + right * moveHorizontal;
    }

    /// <summary>
    /// 处理移动 - 核心移动逻辑
    /// 职责：根据输入计算目标速度，通过平滑过渡更新实际速度
    /// </summary>
    void HandleMovement()
    {
        // 获取当前输入的移动方向（基于摄像机朝向）
        Vector3 direction = GetMovementDirection();

        // 获取当前水平速度（只取X和Z分量，忽略Y轴）
        // 这样跳跃时的垂直速度不会被水平移动逻辑影响
        Vector3 currentVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // 初始化目标速度为零向量
        Vector3 targetVelocity = Vector3.zero;

        // 如果有有效移动输入（有按键按下）
        if (direction.magnitude > 0.01f)
        {
            // 归一化方向向量并乘以最大速度得到目标速度
            // normalized确保方向正确，moveSpeed控制速度大小
            targetVelocity = direction.normalized * moveSpeed;
        }

        // 使用平滑速度变化计算新速度
        // 这个函数处理加速、减速、方向切换的平滑过渡
        Vector3 newVelocity = SmoothVelocityChange(currentVelocity, targetVelocity);

        // 更新刚体速度：
        // X和Z分量使用计算出的新速度（包含平滑过渡）
        // Y分量保持原有值（用于跳跃和下落）
        rb.velocity = new Vector3(newVelocity.x, rb.velocity.y, newVelocity.z);
    }

    /// <summary>
    /// 平滑速度变化 - 核心算法
    /// 使用指数函数实现加速，对数函数实现减速
    /// 支持方向切换时的平滑过渡
    /// </summary>
    /// <param name="current">当前速度向量</param>
    /// <param name="target">目标速度向量</param>
    /// <returns>平滑过渡后的新速度向量</returns>
    Vector3 SmoothVelocityChange(Vector3 current, Vector3 target)
    {
        // 获取当前和目标速度的大小（标量值）
        float currentSpeed = current.magnitude;
        float targetSpeed = target.magnitude;

        // ==================== 有目标速度的情况（有输入） ====================
        if (targetSpeed > 0.01f)
        {
            // 获取目标方向和当前方向
            Vector3 targetDir = target.normalized;
            // 如果当前速度太小（接近零），使用目标方向作为当前方向
            // 避免除零和NaN错误
            Vector3 currentDir = currentSpeed > 0.01f ? current.normalized : targetDir;

            // 计算两个方向向量的点积
            // 点积结果：1表示同向，-1表示完全相反，0表示垂直
            // 用于判断是否需要减速后加速
            float dot = Vector3.Dot(currentDir, targetDir);

            // ==================== 方向相同或夹角小于90度 ====================
            if (dot > 0f)
            {
                // 当前速度与目标速度的比例（用于加速曲线）
                float t = currentSpeed / targetSpeed;

                // 使用指数函数计算加速因子
                // Math.Exp(-x) 的特性：
                // 当x=0时，Exp(0)=1，全速前进
                // 当x>0时，Exp(-x)趋近于0，减速
                // 乘以(1+t)使得速度低时加速更快，速度高时加速趋缓
                float newT = 1f - Mathf.Exp(-accelerationFactor * Time.fixedDeltaTime * (1f + t));

                // 使用Lerp进行速度插值
                // 从当前速度向目标速度插值，newT控制插值程度
                float newSpeed = Mathf.Lerp(currentSpeed, targetSpeed, newT);

                // 返回新速度向量（方向*速度）
                return targetDir * newSpeed;
            }
            // ==================== 方向完全相反（点积为负） ====================
            else
            {
                // 使用对数函数计算减速量
                // Log(1+x) 的特性：
                // 高速时减速量大，低速时减速量小
                // 这样可以实现滑行效果
                float normalizedSpeed = currentSpeed / moveSpeed;
                float logDecel = Mathf.Log(1f + normalizedSpeed * decelerationFactor * 10f) * Time.fixedDeltaTime;

                // 从当前速度减去减速量，确保速度不会小于零
                float newSpeed = Mathf.Max(0f, currentSpeed - logDecel * moveSpeed);

                // 如果减速后还有剩余速度，继续在原方向减速
                if (newSpeed > 0.01f)
                {
                    return currentDir * newSpeed;
                }
                // 速度接近零后，开始向新方向加速
                else
                {
                    // 计算初始加速度，使用较小的值避免突兀
                    float initialAccel = accelerationFactor * Time.fixedDeltaTime * 0.5f;

                    // 新方向的起始速度 = min(目标速度, 初始加速度*最大速度)
                    // 确保不会超过目标速度
                    float startSpeed = Mathf.Min(targetSpeed, initialAccel * moveSpeed);

                    // 开始在新方向加速
                    return targetDir * startSpeed;
                }
            }
        }
        // ==================== 没有目标速度的情况（无输入） ====================
        else
        {
            // 如果还有剩余速度，进行减速
            if (currentSpeed > 0.01f)
            {
                // 归一化当前速度
                float normalizedSpeed = currentSpeed / moveSpeed;

                // 同样使用对数函数减速
                float logDecel = Mathf.Log(1f + normalizedSpeed * decelerationFactor * 10f) * Time.fixedDeltaTime;

                // 计算减速后的新速度
                float newSpeed = Mathf.Max(0f, currentSpeed - logDecel * moveSpeed);

                // 在当前方向继续减速
                return current.normalized * newSpeed;
            }
            // 速度已经接近零，返回零向量
            else
            {
                return Vector3.zero;
            }
        }
    }

    /// <summary>
    /// 处理旋转 - 控制角色朝向
    /// 职责：根据移动方向或速度方向平滑旋转角色
    /// </summary>
    void HandleRotation()
    {
        // 获取输入的移动方向
        Vector3 movementDirection = GetMovementDirection();

        // 获取实际的速度方向（基于物理模拟）
        // 忽略Y轴速度（垂直分量）
        Vector3 velocityDirection = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // 初始化朝向向量
        Vector3 lookDirection = Vector3.zero;

        // 优先使用输入方向（玩家意图）
        if (movementDirection.magnitude > 0.01f)
        {
            lookDirection = movementDirection.normalized;
        }
        // 如果没有输入但有速度，使用速度方向（惯性移动）
        else if (velocityDirection.magnitude > 0.1f)
        {
            lookDirection = velocityDirection.normalized;
        }

        // 如果有有效的朝向方向，执行旋转
        if (lookDirection.magnitude > 0.01f)
        {
            // 计算目标旋转四元数
            // LookRotation根据前方向向量计算旋转
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            // 使用Slerp球面插值实现平滑旋转
            // 最后一个参数控制旋转速度：
            // 值越大，旋转越快，跟随越紧
            // 值越小，旋转越慢，跟随越松
            transform.rotation = Quaternion.Slerp(
                transform.rotation,  // 当前旋转
                targetRotation,      // 目标旋转
                15f * Time.deltaTime // 旋转速度系数
            );
        }
    }

    }
