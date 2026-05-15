using UnityEngine; // 引入Unity引擎命名空间

public class PlayerController : MonoBehaviour // 定义玩家控制器类，继承自MonoBehaviour
{
    [Tooltip("最大移动速度")] // 编辑器提示信息
    public float moveSpeed = 9f; // 玩家最大移动速度
    
    [Tooltip("加速因子（指数曲线）")] // 编辑器提示信息
    public float accelerationFactor = 2f; // 指数加速曲线的控制因子
    
    [Tooltip("减速因子（对数曲线）")] // 编辑器提示信息
    public float decelerationFactor = 0.5f; // 对数减速曲线的控制因子
    
    [Tooltip("摄像机引用")] // 编辑器提示信息
    public Transform cameraTransform; // 摄像机变换组件引用
    
    private Rigidbody rb; // 玩家刚体组件引用
    
    void Start() // Start方法在游戏开始时调用一次
    {
        rb = GetComponent<Rigidbody>(); // 获取玩家对象的刚体组件
        
        if (cameraTransform == null) // 如果摄像机引用未在编辑器中赋值
        {
            if (Camera.main != null) // 如果场景中有主摄像机
                cameraTransform = Camera.main.transform; // 自动获取主摄像机的变换组件
        }
    }
    
    void Update() // Update方法每帧调用一次
    {
        HandleRotation(); // 处理玩家旋转
    }
    
    void FixedUpdate() // FixedUpdate方法按固定时间间隔调用，用于物理计算
    {
        HandleMovement(); // 处理玩家移动
    }
    
    Vector3 GetMovementDirection() // 获取玩家移动方向的方法
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal"); // 获取水平输入（-1到1的原始值）
        float moveVertical = Input.GetAxisRaw("Vertical"); // 获取垂直输入（-1到1的原始值）
        
        Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized; // 计算摄像机前方向在水平面上的投影并归一化
        Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized; // 计算摄像机右方向在水平面上的投影并归一化
        
        return forward * moveVertical + right * moveHorizontal; // 组合水平和垂直输入，返回最终移动方向
    }
    
    void HandleMovement() // 处理玩家移动的方法
    {
        Vector3 direction = GetMovementDirection(); // 获取当前输入的移动方向
        Vector3 currentVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); // 获取当前水平速度（忽略Y轴）
        Vector3 targetVelocity = Vector3.zero; // 初始化目标速度为零
        
        if (direction.magnitude > 0.01f) // 如果有移动输入
        {
            targetVelocity = direction.normalized * moveSpeed; // 计算目标速度（方向归一化乘以最大速度）
        }
        
        Vector3 newVelocity = SmoothVelocityChange(currentVelocity, targetVelocity); // 计算平滑过渡后的新速度
        rb.velocity = new Vector3(newVelocity.x, rb.velocity.y, newVelocity.z); // 更新刚体速度（保留Y轴速度）
    }
    
    Vector3 SmoothVelocityChange(Vector3 current, Vector3 target) // 平滑速度变化的方法，处理加速和减速
    {
        float currentSpeed = current.magnitude; // 获取当前速度大小
        float targetSpeed = target.magnitude; // 获取目标速度大小
        
        if (targetSpeed > 0.01f) // 如果有目标速度（有输入）
        {
            Vector3 targetDir = target.normalized; // 获取目标方向
            Vector3 currentDir = currentSpeed > 0.01f ? current.normalized : targetDir; // 获取当前方向，如果当前速度为零则使用目标方向
            
            float dot = Vector3.Dot(currentDir, targetDir); // 计算当前方向和目标方向的点积（判断方向是否一致）
            
            if (dot > 0f) // 如果方向相同或夹角小于90度（不是完全相反）
            {
                float t = currentSpeed / targetSpeed; // 计算当前速度与目标速度的比例
                float newT = 1f - Mathf.Exp(-accelerationFactor * Time.fixedDeltaTime * (1f + t)); // 使用指数函数计算加速因子
                float newSpeed = Mathf.Lerp(currentSpeed, targetSpeed, newT); // 插值计算新速度
                return targetDir * newSpeed; // 返回新速度向量
            }
            else // 如果方向完全相反（点积为负）
            {
                float decelAmount = decelerationFactor * Time.fixedDeltaTime; // 计算减速量
                float normalizedSpeed = currentSpeed / moveSpeed; // 计算归一化速度
                float logDecel = Mathf.Log(1f + normalizedSpeed * decelerationFactor * 10f) * Time.fixedDeltaTime; // 使用对数函数计算减速量
                float newSpeed = Mathf.Max(0f, currentSpeed - logDecel * moveSpeed); // 计算减速后的新速度
                
                if (newSpeed > 0.01f) // 如果还有剩余速度
                {
                    return currentDir * newSpeed; // 继续在原方向减速
                }
                else // 速度接近零后
                {
                    float initialAccel = accelerationFactor * Time.fixedDeltaTime * 0.5f; // 计算初始加速度
                    float startSpeed = Mathf.Min(targetSpeed, initialAccel * moveSpeed); // 计算新方向的起始速度
                    return targetDir * startSpeed; // 开始在新方向加速
                }
            }
        }
        else // 没有目标速度（无输入）
        {
            if (currentSpeed > 0.01f) // 如果还有速度
            {
                float normalizedSpeed = currentSpeed / moveSpeed; // 计算归一化速度
                float logDecel = Mathf.Log(1f + normalizedSpeed * decelerationFactor * 10f) * Time.fixedDeltaTime; // 使用对数函数减速
                float newSpeed = Mathf.Max(0f, currentSpeed - logDecel * moveSpeed); // 计算减速后的新速度
                return current.normalized * newSpeed; // 在当前方向继续减速
            }
            else // 速度接近零
            {
                return Vector3.zero; // 速度设为零
            }
        }
    }
    
    void HandleRotation() // 处理玩家旋转的方法
    {
        Vector3 movementDirection = GetMovementDirection(); // 获取输入的移动方向
        Vector3 velocityDirection = new Vector3(rb.velocity.x, 0, rb.velocity.z); // 获取实际移动方向
        
        Vector3 lookDirection = Vector3.zero;
        
        if (movementDirection.magnitude > 0.01f)
        {
            lookDirection = movementDirection.normalized;
        }
        else if (velocityDirection.magnitude > 0.1f)
        {
            lookDirection = velocityDirection.normalized;
        }
        
        if (lookDirection.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
        }
    }
}
