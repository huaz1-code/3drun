using UnityEngine;

/// <summary>
/// 来回移动障碍物脚本 - 让物体沿 X 方向在固定范围内匀速来回移动
/// 功能说明：
/// 1. 以初始位置为中心，沿 X 轴在 ±range/2 范围内来回移动
/// 2. 到达边界时反向运动（匀速反弹，无缓动）
/// 3. 可选是否在 Update 中执行（默认）或 FixedUpdate 中执行
/// 4. 玩家进入触发器时扣血（需将 Collider 设为 Is Trigger）
/// </summary>
public class BackAndForthObstacle : MonoBehaviour
{
    /// <summary>
    /// 移动速度 - 沿 X 方向的运动速度（米/秒）
    /// </summary>
    [Tooltip("沿 X 方向的移动速度（米/秒）")]
    public float moveSpeed = 2f;

    /// <summary>
    /// 移动范围 - 以初始位置为中心沿 X 方向的总行程长度（米）
    /// 实际移动区间为 [起始X - range/2, 起始X + range/2]
    /// </summary>
    [Tooltip("以初始位置为中心沿 X 方向的总行程长度（米）")]
    public float range = 5f;

    /// <summary>
    /// 是否在 FixedUpdate 中执行 - 物理类障碍物建议勾选
    /// 关闭则在 Update 中执行
    /// </summary>
    [Tooltip("是否在 FixedUpdate 中执行（物理类障碍物建议勾选）")]
    public bool useFixedUpdate = false;

    /// <summary>
    /// 攻击伤害值 - 玩家碰到障碍物时扣除的血量
    /// </summary>
    [Tooltip("玩家碰到时扣除的血量")]
    public float attackDamage = 20f;

    /// <summary>
    /// 伤害冷却时间 - 两次伤害之间的最小间隔（秒）
    /// 防止玩家在触发器内持续被扣血
    /// </summary>
    [Tooltip("两次伤害之间的最小间隔时间（秒）")]
    public float damageCooldown = 1f;

    /// <summary>
    /// 初始 X 坐标 - 游戏开始时物体的 X 位置，作为移动中心
    /// </summary>
    private float startX;

    /// <summary>
    /// 当前移动方向 - 1 表示 X 正方向，-1 表示 X 负方向
    /// </summary>
    private int direction = 1;

    /// <summary>
    /// 上次伤害时间戳 - 用于实现伤害冷却
    /// </summary>
    private float lastDamageTime = -999f;

    /// <summary>
    /// 初始化方法 - 游戏开始时调用一次
    /// 职责：记录初始 X 坐标作为移动中心
    /// </summary>
    void Start()
    {
        startX = transform.position.x;
    }

    /// <summary>
    /// 更新方法 - 每帧调用一次
    /// 职责：当未启用 FixedUpdate 模式时在此处处理移动
    /// </summary>
    void Update()
    {
        if (!useFixedUpdate)
        {
            Move(Time.deltaTime);
        }
    }

    /// <summary>
    /// 物理更新方法 - 按固定时间间隔调用
    /// 职责：当启用 FixedUpdate 模式时在此处处理移动
    /// </summary>
    void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            Move(Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// 玩家进入触发区域时扣血
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
        // 冷却检查
        if (Time.time - lastDamageTime < damageCooldown)
        {
            return;
        }

        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth == null || playerHealth.IsDead()) return;

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null && playerController.IsInvincible()) return;

        // 护盾检查
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
    /// 移动方法 - 按时间增量沿 X 方向移动，并在到达边界时反向
    /// </summary>
    /// <param name="deltaTime">本帧时间增量</param>
    private void Move(float deltaTime)
    {
        // 计算本帧 X 方向位移
        float deltaX = direction * moveSpeed * deltaTime;

        // 计算新位置
        Vector3 newPosition = transform.position;
        newPosition.x += deltaX;

        // 边界检查：到达任一端点时反转方向并夹紧位置
        float minX = startX - range / 2f;
        float maxX = startX + range / 2f;

        if (newPosition.x >= maxX)
        {
            newPosition.x = maxX;
            direction = -1;
        }
        else if (newPosition.x <= minX)
        {
            newPosition.x = minX;
            direction = 1;
        }

        // 应用新位置
        transform.position = newPosition;
    }

    /// <summary>
    /// Gizmos 绘制方法 - 在 Scene 视图中显示移动范围
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 在编辑器未运行时，使用当前 X 作为中心预览；运行时使用实际起始 X
        float centerX = Application.isPlaying ? startX : transform.position.x;

        Vector3 center = new Vector3(centerX, transform.position.y, transform.position.z);
        Vector3 left = new Vector3(centerX - range / 2f, transform.position.y, transform.position.z);
        Vector3 right = new Vector3(centerX + range / 2f, transform.position.y, transform.position.z);

        // 绘制移动范围线段（黄色）
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(left, right);

        // 绘制两端端点（黄色球体）
        Gizmos.DrawSphere(left, 0.1f);
        Gizmos.DrawSphere(right, 0.1f);

        // 绘制中心点（红色球体）标识起始位置
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(center, 0.1f);
    }
}
