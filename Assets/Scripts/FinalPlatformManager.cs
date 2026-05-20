using UnityEngine;

/// <summary>
/// 最终平台管理器脚本 - 控制平台生成的终止和最终平台的生成
/// 功能说明：
/// 1. 监听平台生成事件
/// 2. 当生成到第20个平台时停止生成普通平台
/// 3. 在第20个平台位置显示大圆柱平台（场景中已有的对象）
/// </summary>
public class FinalPlatformManager : MonoBehaviour
{
    /// <summary>
    /// 目标平台数量 - 到达此数量后停止生成普通平台并显示圆柱
    /// </summary>
    [Tooltip("目标平台数量（到达后停止生成并显示圆柱）")]
    public int targetPlatformCount = 20;

    /// <summary>
    /// 场景中已有的圆柱平台对象 - 开局隐藏，到达目标平台时显示
    /// </summary>
    [Tooltip("场景中已有的圆柱平台对象（开局隐藏，到达目标平台时显示）")]
    public GameObject cylinderPlatform;

    /// <summary>
    /// 平台宽度 - 用于计算圆柱与平台之间的距离
    /// </summary>
    [Tooltip("平台宽度（用于计算间距）")]
    public float platformWidth = 2f;

    /// <summary>
    /// 额外间距 - 平台与圆柱之间的空隙
    /// </summary>
    [Tooltip("平台与圆柱之间的额外空隙")]
    public float extraGap = 1f;

    /// <summary>
    /// 是否已经显示过最终平台
    /// </summary>
    private bool hasShownFinalPlatform = false;

    /// <summary>
    /// 平台生成器引用
    /// </summary>
    private PlatformSpawner platformSpawner;

    /// <summary>
    /// 初始化方法
    /// </summary>
    void Start()
    {
        // 查找平台生成器
        platformSpawner = FindObjectOfType<PlatformSpawner>();
        
        if (platformSpawner == null)
        {
            Debug.LogError("FinalPlatformManager: 未找到PlatformSpawner！");
        }

        // 开局隐藏圆柱平台（像NPCPrefab那样）
        if (cylinderPlatform != null)
        {
            cylinderPlatform.SetActive(false);
            Debug.Log("FinalPlatformManager: 已隐藏圆柱平台");
        }
    }

    /// <summary>
    /// 更新方法 - 检查平台数量
    /// </summary>
    void Update()
    {
        // 如果已经显示过最终平台，不再处理
        if (hasShownFinalPlatform) return;

        // 检查平台生成器是否存在
        if (platformSpawner == null)
        {
            platformSpawner = FindObjectOfType<PlatformSpawner>();
            if (platformSpawner == null) return;
        }

        // 获取当前平台索引（即已生成的平台数量）
        int platformIndex = GetPlatformIndex();

        // 如果到达目标平台数量，显示最终平台
        if (platformIndex >= targetPlatformCount)
        {
            ShowFinalPlatform();
        }
    }

    /// <summary>
    /// 获取当前平台索引
    /// </summary>
    int GetPlatformIndex()
    {
        // 通过反射获取平台生成器的私有字段platformIndex
        System.Reflection.FieldInfo field = typeof(PlatformSpawner).GetField("platformIndex", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null && platformSpawner != null)
        {
            return (int)field.GetValue(platformSpawner);
        }
        
        return 0;
    }

    /// <summary>
    /// 显示最终平台（在第20个平台位置）
    /// </summary>
    void ShowFinalPlatform()
    {
        hasShownFinalPlatform = true;

        // 禁用平台生成器，停止生成普通平台
        if (platformSpawner != null)
        {
            platformSpawner.enabled = false;
            Debug.Log("FinalPlatformManager: 已停止生成普通平台");
        }

        // 获取第20个平台的位置
        Vector3 platformPosition = GetPlatformPosition(targetPlatformCount - 1);

        // 如果设置了圆柱平台对象，显示它并移动到目标位置
        if (cylinderPlatform != null)
        {
            // 计算圆柱平台位置：与第20个平台在同一高度，中心距离为圆柱半径+平台宽度
            Vector3 cylinderPosition = CalculateCylinderPosition(platformPosition);
            
            // 设置圆柱平台位置（与平台同一高度，不向上偏移）
            cylinderPlatform.transform.position = cylinderPosition;
            
            // 显示圆柱平台
            cylinderPlatform.SetActive(true);
            
            Debug.Log($"FinalPlatformManager: 已在位置 {cylinderPosition} 显示圆柱平台");
        }
        else
        {
            Debug.LogError("FinalPlatformManager: 未设置cylinderPlatform！");
        }
    }

    /// <summary>
    /// 获取指定索引平台的位置（螺旋生成位置）
    /// </summary>
    Vector3 GetPlatformPosition(int platformIndex)
    {
        // 通过反射获取平台生成器的字段
        System.Reflection.FieldInfo centerField = typeof(PlatformSpawner).GetField("cylinderCenter", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo radiusField = typeof(PlatformSpawner).GetField("spiralRadius", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo heightField = typeof(PlatformSpawner).GetField("heightPerPlatform", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        System.Reflection.FieldInfo rotationField = typeof(PlatformSpawner).GetField("rotationPerPlatform", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // 获取圆柱体中心位置
        Vector3 centerPos = Vector3.zero;
        if (centerField != null && platformSpawner != null)
        {
            Transform center = (Transform)centerField.GetValue(platformSpawner);
            if (center != null)
            {
                centerPos = center.position;
            }
        }

        // 获取螺旋半径
        float spiralRadius = 6f;
        if (radiusField != null && platformSpawner != null)
        {
            spiralRadius = (float)radiusField.GetValue(platformSpawner);
        }

        // 获取每平台上升高度
        float heightPerPlatform = 1.5f;
        if (heightField != null && platformSpawner != null)
        {
            heightPerPlatform = (float)heightField.GetValue(platformSpawner);
        }

        // 获取每平台旋转角度
        float rotationPerPlatform = 45f;
        if (rotationField != null && platformSpawner != null)
        {
            rotationPerPlatform = (float)rotationField.GetValue(platformSpawner);
        }

        // 计算平台位置（螺旋公式）
        float angle = platformIndex * rotationPerPlatform * Mathf.Deg2Rad;
        float x = centerPos.x + spiralRadius * Mathf.Cos(angle);
        float z = centerPos.z + spiralRadius * Mathf.Sin(angle);
        float y = centerPos.y + platformIndex * heightPerPlatform;

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// 计算圆柱平台位置：与平台同一高度，中心距离为圆柱半径+平台宽度+额外间距
    /// </summary>
    Vector3 CalculateCylinderPosition(Vector3 platformPosition)
    {
        // 通过反射获取平台生成器的字段
        System.Reflection.FieldInfo centerField = typeof(PlatformSpawner).GetField("cylinderCenter", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        // 获取圆柱体中心位置
        Vector3 centerPos = Vector3.zero;
        if (centerField != null && platformSpawner != null)
        {
            Transform center = (Transform)centerField.GetValue(platformSpawner);
            if (center != null)
            {
                centerPos = center.position;
            }
        }

        // 获取圆柱半径（从对象的缩放或碰撞体获取）
        float cylinderRadius = 5f; // 默认值
        if (cylinderPlatform != null)
        {
            // 尝试从碰撞体获取半径
            Collider collider = cylinderPlatform.GetComponent<Collider>();
            if (collider is SphereCollider sphereCollider)
            {
                cylinderRadius = sphereCollider.radius * cylinderPlatform.transform.localScale.x;
            }
            else if (collider is CapsuleCollider capsuleCollider)
            {
                cylinderRadius = capsuleCollider.radius * cylinderPlatform.transform.localScale.x;
            }
            else if (collider is MeshCollider)
            {
                // 使用缩放作为半径估算
                cylinderRadius = cylinderPlatform.transform.localScale.x / 2f;
            }
        }

        // 计算从平台到圆柱中心的方向（背离圆柱体中心）
        Vector3 directionFromCenter = (platformPosition - centerPos).normalized;
        
        // 圆柱中心位置 = 平台位置 + 方向 * (圆柱半径 + 平台宽度/2 + 额外间距)
        // 使用平台宽度的一半，因为平台位置是中心位置
        float totalDistance = cylinderRadius + (platformWidth / 2f) + extraGap;
        Vector3 cylinderPosition = platformPosition + directionFromCenter * totalDistance;
        
        // 保持与平台同一高度
        cylinderPosition.y = platformPosition.y;

        return cylinderPosition;
    }

    }