using UnityEngine;

public enum PlatformType
{
    Normal,
    Damage,
    Temporary
}

public class Platform : MonoBehaviour
{
    [Tooltip("普通平台材质")]
    public Material normalMaterial;

    [Tooltip("伤害平台材质（红色）")]
    public Material damageMaterial;

    [Tooltip("临时平台材质（半透明）")]
    public Material temporaryMaterial;

    [Tooltip("伤害平台每次伤害值")]
    public int damageAmount = 1;

    [Tooltip("伤害平台伤害间隔时间（秒）")]
    public float damageInterval = 0.5f;

    [Tooltip("临时平台持续时间（秒）")]
    public float temporaryDuration = 3f;

    [Tooltip("调试模式")]
    public bool debugMode = false;

    private PlayerJump playerJump;
    private GameObject player;

    private Renderer platformRenderer;
    private Collider platformCollider;
    private PlatformType currentType;
    private float temporaryTimer;
    private float damageTimer;
    private bool hasStartedCountdown;
    private bool wasPlayerOnPlatform;
    private bool isPlayerOnPlatform;

    private GameObject currentCoin;

    /// <summary>
    /// NPC存在标记 - 表示平台上是否有NPC
    /// 用于避免在有NPC的平台上生成金币
    /// </summary>
    public bool HasNPC { get; set; }

    void Awake()
    {
        platformRenderer = GetComponent<Renderer>();
        platformCollider = GetComponent<Collider>();
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerJump = player.GetComponent<PlayerJump>();
        }
    }

    public void ResetPlatform()
    {
        if (debugMode) Debug.Log($"Platform.ResetPlatform: {name}, 金币: {currentCoin}");

        RecycleCoin();

        HasNPC = false;

        temporaryTimer = 0f;
        damageTimer = 0f;
        hasStartedCountdown = false;
        platformCollider.enabled = true;
        platformRenderer.enabled = true;

        SetPlatformType(PlatformType.Normal);
    }

    public void SetPlatformType(PlatformType type)
    {
        currentType = type;

        switch (type)
        {
            case PlatformType.Normal:
                SetMaterial(normalMaterial);
                ResetPhysicMaterial();
                break;
            case PlatformType.Damage:
                SetMaterial(damageMaterial);
                ResetPhysicMaterial();
                break;
            case PlatformType.Temporary:
                SetMaterial(temporaryMaterial);
                ResetPhysicMaterial();
                temporaryTimer = temporaryDuration;
                hasStartedCountdown = false;
                break;
        }
    }

    private void SetMaterial(Material material)
    {
        if (platformRenderer != null && material != null)
        {
            platformRenderer.material = material;
        }
    }

    private void SetPhysicMaterial(PhysicMaterial physicMaterial)
    {
        if (platformCollider != null && physicMaterial != null)
        {
            platformCollider.material = physicMaterial;
        }
    }

    private void ResetPhysicMaterial()
    {
        if (platformCollider != null)
        {
            platformCollider.material = null;
        }
    }

    void Update()
    {
        wasPlayerOnPlatform = isPlayerOnPlatform;
        isPlayerOnPlatform = IsPlayerOnPlatform();

        if (isPlayerOnPlatform && !wasPlayerOnPlatform)
        {
            OnPlayerEnterPlatform();
        }

        if (!isPlayerOnPlatform && wasPlayerOnPlatform)
        {
            OnPlayerExitPlatform();
        }

        if (currentType == PlatformType.Temporary && hasStartedCountdown)
        {
            temporaryTimer -= Time.deltaTime;
            if (temporaryTimer <= 0f)
            {
                RecycleCoin();
                PlatformPool.Instance.ReturnPlatform(gameObject);
                Debug.Log($"临时平台 {name} 已自动回收");
            }
        }

        if (currentType == PlatformType.Damage && isPlayerOnPlatform)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                ApplyDamageToPlayer();
            }
        }
    }

    bool IsPlayerOnPlatform()
    {
        if (playerJump == null || player == null || playerJump.groundLayer.value == 0)
            return false;

        Vector3 center = player.transform.position + playerJump.groundCheckOffset;

        Collider[] hitColliders = Physics.OverlapBox(
            center,
            playerJump.groundCheckHalfExtents,
            player.transform.rotation,
            playerJump.groundLayer
        );

        foreach (var collider in hitColliders)
        {
            if (collider.gameObject == gameObject)
            {
                return true;
            }
        }

        return false;
    }

    void OnPlayerEnterPlatform()
    {
        Debug.Log($"玩家跳上了平台: {name}, 类型: {currentType}");

        if (currentType == PlatformType.Damage)
        {
            damageTimer = 0f;
        }
        else if (currentType == PlatformType.Temporary && !hasStartedCountdown)
        {
            hasStartedCountdown = true;
            temporaryTimer = temporaryDuration;
            Debug.Log($"临时平台 {name} 开始倒计时: {temporaryDuration}秒");
        }
    }

    void ApplyDamageToPlayer()
    {
        if (playerJump == null || player == null)
            return;

        Health health = player.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damageAmount);
            Debug.Log($"伤害平台 {name} 对玩家造成 {damageAmount} 点伤害");
        }
    }

    void OnPlayerExitPlatform()
    {
    }

    /// <summary>
    /// 检查是否有金币 - 判断平台上当前是否持有金币
    /// </summary>
    /// <returns>true表示有金币，false表示没有</returns>
    public bool HasCoin()
    {
        // currentCoin为null时返回false
        return currentCoin != null;
    }

    /// <summary>
    /// 设置金币 - 在平台上放置金币
    /// </summary>
    /// <param name="coin">要设置的金币GameObject</param>
    public void SetCoin(GameObject coin)
    {
        // 调试日志
        if (debugMode && coin != null)
            Debug.Log($"Platform.SetCoin: {name} 设置金币 {coin.name}");

        // 记录金币引用
        currentCoin = coin;
    }

    /// <summary>
    /// 回收金币 - 将金币归还到对象池
    /// 如果没有金币则跳过
    /// </summary>
    public void RecycleCoin()
    {
        // 检查是否有金币需要回收
        if (currentCoin != null)
        {
            // 调试日志
            if (debugMode)
                Debug.Log($"Platform.RecycleCoin: {name} 回收金币 {currentCoin.name}");

            // 检查金币池是否存在
            if (CoinPool.Instance != null)
            {
                // 调用金币池的回收方法
                // 金币池负责将金币设为非激活状态并放回队列
                CoinPool.Instance.ReturnCoin(currentCoin);
            }

            // 清空引用
            currentCoin = null;
        }
        else if (debugMode)
        {
            // 没有金币时的调试日志
            Debug.Log($"Platform.RecycleCoin: {name} 没有金币可回收");
        }
    }

    /// <summary>
    /// 销毁回调 - 当平台被销毁时调用
    /// 确保平台销毁时金币也被正确回收
    /// 防止内存泄漏
    /// </summary>
    void OnDestroy()
    {
        // 调用回收方法，确保子对象被正确清理
        RecycleCoin();
    }
}
