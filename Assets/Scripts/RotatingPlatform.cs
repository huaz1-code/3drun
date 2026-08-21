using UnityEngine;

/// <summary>
/// 旋转平台脚本 - 让平台有规律地翻转并间歇停止
/// 功能说明：
/// 1. 绕指定轴旋转一定角度后停止，再反向旋转回来，交替进行
/// 2. 使用相对角度累加方式实现旋转，避免欧拉角环绕问题
/// 3. 可配置旋转速度、翻转角度、停止时长和旋转轴
/// </summary>
public class RotatingPlatform : MonoBehaviour
{
    /// <summary>
    /// 旋转速度 - 每秒旋转的角度（度/秒）
    /// </summary>
    [Tooltip("每秒旋转的角度（度/秒）")]
    public float rotationSpeed = 90f;

    /// <summary>
    /// 翻转角度 - 每次翻转旋转的角度（度），默认180°表示翻一面
    /// </summary>
    [Tooltip("每次翻转旋转的角度（度）")]
    public float flipAngle = 180f;

    /// <summary>
    /// 停止时长 - 每次翻完后停留的时间（秒）
    /// </summary>
    [Tooltip("每次翻转后停留的时间（秒）")]
    public float stopDuration = 1f;

    /// <summary>
    /// 旋转轴 - 绕哪个轴旋转（0=X轴, 1=Y轴, 2=Z轴）
    /// </summary>
    [Tooltip("绕哪个轴旋转（X轴=0, Y轴=1, Z轴=2）")]
    public int rotationAxis = 0;

    /// <summary>
    /// 攻击伤害值 - 玩家碰到触发器时扣除的血量
    /// </summary>
    [Tooltip("玩家碰到DamageCheck触发器时扣除的血量")]
    public float attackDamage = 20f;

    /// <summary>
    /// 伤害冷却时间 - 两次伤害之间的最小间隔（秒）
    /// </summary>
    [Tooltip("两次伤害之间的最小间隔时间（秒）")]
    public float damageCooldown = 1f;

    /// <summary>
    /// 上次伤害时间戳 - 用于实现伤害冷却
    /// </summary>
    private float lastDamageTime = -999f;

    /// <summary>
    /// 初始旋转 - 游戏开始时物体的原始姿态
    /// </summary>
    private Quaternion startRotation;

    /// <summary>
    /// 当前相对角度 - 相对于初始姿态的旋转角度（可正可负）
    /// </summary>
    private float currentRelativeAngle = 0f;

    /// <summary>
    /// 旋转方向 - 1表示正向旋转，-1表示反向旋转
    /// </summary>
    private int direction = 1;

    /// <summary>
    /// 当前旋转的目标角度 - 相对于初始姿态的绝对目标角度
    /// 每次翻转后重新计算为 currentRelativeAngle + direction * flipAngle
    /// </summary>
    private float targetAngle;

    /// <summary>
    /// 停止计时器 - 停止状态下累计的时间
    /// </summary>
    private float stopTimer = 0f;

    /// <summary>
    /// 当前状态 - true=正在旋转，false=正在停止
    /// </summary>
    private bool isRotating = true;

    /// <summary>
    /// 旋转轴的单位向量
    /// </summary>
    private Vector3 AxisVector
    {
        get
        {
            switch (rotationAxis)
            {
                case 0: return Vector3.right;
                case 1: return Vector3.up;
                case 2: return Vector3.forward;
                default: return Vector3.right;
            }
        }
    }

    /// <summary>
    /// 初始化方法 - 游戏开始时调用一次
    /// 职责：记录初始旋转姿态
    /// </summary>
    void Start()
    {
        startRotation = transform.rotation;
        targetAngle = flipAngle;
    }

    /// <summary>
    /// 更新方法 - 每帧调用一次
    /// 职责：根据当前状态执行旋转或停止逻辑
    /// </summary>
    void Update()
    {
        if (isRotating)
        {
            UpdateRotation(Time.deltaTime);
        }
        else
        {
            UpdateStop(Time.deltaTime);
        }
    }

    /// <summary>
    /// 旋转更新 - 按旋转速度更新相对角度，并检测是否到达翻转目标
    /// </summary>
    /// <param name="deltaTime">本帧时间增量</param>
    private void UpdateRotation(float deltaTime)
    {
        float angleStep = rotationSpeed * deltaTime * direction;
        float newAngle = currentRelativeAngle + angleStep;

        // 检查是否到达目标角度
        if (direction > 0 && newAngle >= targetAngle)
        {
            newAngle = targetAngle;
            ApplyRotation(newAngle);
            isRotating = false;
            stopTimer = 0f;
            return;
        }
        else if (direction < 0 && newAngle <= targetAngle)
        {
            newAngle = targetAngle;
            ApplyRotation(newAngle);
            isRotating = false;
            stopTimer = 0f;
            return;
        }

        currentRelativeAngle = newAngle;
        ApplyRotation(currentRelativeAngle);
    }

    /// <summary>
    /// 停止更新 - 累计停止时间，到达设定时长后切换回旋转状态并反向
    /// </summary>
    /// <param name="deltaTime">本帧时间增量</param>
    private void UpdateStop(float deltaTime)
    {
        stopTimer += deltaTime;

        if (stopTimer >= stopDuration)
        {
            direction *= -1;
            targetAngle = currentRelativeAngle + direction * flipAngle;
            isRotating = true;
        }
    }

    /// <summary>
    /// 应用旋转 - 基于初始姿态和相对角度计算最终旋转
    /// 使用四元数乘法避免欧拉角环绕问题
    /// </summary>
    /// <param name="relativeAngle">相对初始姿态的旋转角度</param>
    private void ApplyRotation(float relativeAngle)
    {
        transform.rotation = startRotation * Quaternion.AngleAxis(relativeAngle, AxisVector);
    }

    /// <summary>
    /// 玩家进入触发区域时扣血
    /// 触发器需设在子对象DamageCheck上，父对象需有Kinematic Rigidbody使事件传递到父脚本
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TryDamagePlayer(other.gameObject);
        }
    }

    /// <summary>
    /// 玩家停留在触发区域时持续检测冷却后扣血
    /// </summary>
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TryDamagePlayer(other.gameObject);
        }
    }

    /// <summary>
    /// 尝试对玩家造成伤害 - 包含冷却、死亡、无敌、护盾检查
    /// </summary>
    /// <param name="player">玩家GameObject</param>
    private void TryDamagePlayer(GameObject player)
    {
        if (Time.time - lastDamageTime < damageCooldown)
        {
            return;
        }

        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth == null || playerHealth.IsDead()) return;

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null && playerController.IsInvincible()) return;

        PotionEffects potionEffects = player.GetComponent<PotionEffects>();
        if (potionEffects != null && potionEffects.CheckAndConsumeShield())
        {
            playerController?.EnterInvincibility();
            lastDamageTime = Time.time;
            return;
        }

        playerHealth.TakeDamage(attackDamage);
        lastDamageTime = Time.time;
    }

    /// <summary>
    /// Gizmos 绘制方法 - 在 Scene 视图中显示旋转轴和翻转范围
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Vector3 axis = AxisVector;
        Vector3 origin = transform.position;

        // 绘制旋转轴（红色）
        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, axis * 2f);
        Gizmos.DrawRay(origin, -axis * 2f);

        // 绘制翻转范围（绿色弧线示意）
        Gizmos.color = Color.green;
        Vector3 startPos = origin;
        Vector3 endPos = origin;

        // 用简单线框表示翻转角度
        Vector3 perpendicular;
        if (axis == Vector3.right)
            perpendicular = Vector3.forward;
        else if (axis == Vector3.up)
            perpendicular = Vector3.right;
        else
            perpendicular = Vector3.up;

        float radius = 1.5f;
        Vector3 arcStart = origin + perpendicular * radius;
        Vector3 arcEnd = origin + Quaternion.AngleAxis(flipAngle, axis) * perpendicular * radius;

        Gizmos.DrawLine(origin, arcStart);
        Gizmos.DrawLine(origin, arcEnd);

        // 标注初始方向
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(arcStart, 0.05f);

        // 标注翻转终点
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(arcEnd, 0.05f);
    }
}
