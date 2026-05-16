using UnityEngine;

/// <summary>
/// 摄像机控制器脚本 - 实现第三人称跟随摄像机
/// 功能说明：
/// 1. 跟随玩家位置（带平滑过渡）
/// 2. 支持鼠标控制视角旋转
/// 3. 可调节的摄像机距离和高度
/// 4. 限制垂直旋转角度范围
/// </summary>
public class CameraController : MonoBehaviour
{
    /// <summary>
    /// 跟随目标 - 摄像机要跟随的对象
    /// 通常是玩家对象
    /// </summary>
    [Tooltip("跟随的目标")]
    public Transform target;

    /// <summary>
    /// 摄像机与目标的距离 - 沿摄像机朝向的距离
    /// 值越大，摄像机离目标越远
    /// </summary>
    [Tooltip("摄像机与目标的距离")]
    public float distance = 5f;

    /// <summary>
    /// 摄像机高度偏移 - 相对于目标中心的垂直偏移
    /// 正值表示摄像机在目标上方
    /// </summary>
    [Tooltip("摄像机高度偏移")]
    public float height = 2f;

    /// <summary>
    /// 水平旋转速度 - 鼠标X轴控制旋转的速度
    /// 值越大，旋转越快
    /// </summary>
    [Tooltip("水平旋转速度")]
    public float rotationSpeedX = 2f;

    /// <summary>
    /// 垂直旋转速度 - 鼠标Y轴控制俯仰的速度
    /// 值越大，俯仰变化越快
    /// </summary>
    [Tooltip("垂直旋转速度")]
    public float rotationSpeedY = 1.5f;

    /// <summary>
    /// 最小俯视角度 - 垂直旋转的最小角度（负数表示向下看）
    /// 防止摄像机翻转到目标下方
    /// </summary>
    [Tooltip("最小俯视角度")]
    public float minVerticalAngle = -30f;

    /// <summary>
    /// 最大仰视角度 - 垂直旋转的最大角度（正数表示向上看）
    /// 防止摄像机翻转到目标正上方
    /// </summary>
    [Tooltip("最大仰视角度")]
    public float maxVerticalAngle = 60f;

    /// <summary>
    /// 平滑跟随速度 - 摄像机位置过渡的速度系数
    /// 值越大，跟随越快但可能产生抖动
    /// 值越小，跟随越平滑但可能有滞后感
    /// </summary>
    [Tooltip("平滑跟随速度")]
    public float followSmoothSpeed = 5f;

    /// <summary>
    /// 当前水平角度 - 摄像机绕Y轴的旋转角度
    /// 用于累积鼠标X轴输入
    /// </summary>
    private float currentHorizontalAngle = 0f;

    /// <summary>
    /// 当前垂直角度 - 摄像机绕X轴的旋转角度（俯仰角）
    /// 用于累积鼠标Y轴输入
    /// </summary>
    private float currentVerticalAngle = 10f;

    /// <summary>
    /// 初始化方法 - 游戏开始时调用一次
    /// 职责：查找跟随目标，锁定鼠标
    /// </summary>
    void Start()
    {
        // 如果没有手动指定目标，尝试通过名称查找
        if (target == null)
        {
            GameObject player = GameObject.Find("player");
            if (player != null)
                target = player.transform;
        }

        // 锁定鼠标到游戏窗口中心
        // CursorLockMode.Locked：鼠标被锁定在窗口中心，光标不可见
        Cursor.lockState = CursorLockMode.Locked;

        // 隐藏鼠标光标
        Cursor.visible = false;
    }

    /// <summary>
    /// 延迟更新方法 - 在所有Update方法之后调用
    /// 职责：处理摄像机位置和旋转（跟随目标）
    /// 使用LateUpdate确保玩家位置已更新完成
    /// </summary>
    void LateUpdate()
    {
        // 如果没有目标，直接返回
        if (target == null) return;

        // ========== 获取鼠标输入 ==========
        // GetAxis 返回平滑的输入值（-1到1）
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // ========== 更新旋转角度 ==========
        // 水平旋转：累积鼠标X轴输入
        currentHorizontalAngle += mouseX * rotationSpeedX;

        // 垂直旋转：累积鼠标Y轴输入，并限制范围
        // 注意：鼠标Y轴是反向的，所以用减法
        currentVerticalAngle -= mouseY * rotationSpeedY;

        // 限制垂直角度在指定范围内
        // Mathf.Clamp 用于限制值在最小和最大值之间
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);

        // ========== 计算摄像机朝向 ==========
        // 使用欧拉角创建旋转四元数
        // 参数顺序：X（俯仰），Y（偏航），Z（翻滚）
        Quaternion rotation = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle, 0);

        // ========== 计算摄像机位置 ==========
        // 目标位置 = 玩家位置 + 向上偏移
        Vector3 targetPosition = target.position + Vector3.up * height;

        // 期望位置 = 目标位置 - (旋转方向 * 距离)
        // 旋转的forward向量表示摄像机应该看的方向
        // 乘以距离得到摄像机应该在的位置
        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward) * distance;

        // ========== 平滑移动到目标位置 ==========
        // Vector3.Lerp 线性插值两个位置
        // Time.deltaTime 控制每帧的移动量
        // 公式：当前位置 + (目标位置 - 当前位置) * 速度系数 * 时间
        transform.position = Vector3.Lerp(
            transform.position,     // 当前摄像机位置
            desiredPosition,       // 期望的摄像机位置
            followSmoothSpeed * Time.deltaTime  // 平滑系数
        );

        // ========== 应用旋转 ==========
        // 直接设置旋转，不使用插值
        // 因为旋转是由鼠标输入直接控制的
        transform.rotation = rotation;
    }
}
