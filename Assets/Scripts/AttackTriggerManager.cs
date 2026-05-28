using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 攻击触发器管理器 - 管理多个AttackTrigger之间的连线
/// 功能说明：
/// 1. 跟踪所有激活的AttackTrigger
/// 2. 当两个触发器激活时，在它们之间绘制连线
/// 3. 当玩家或Boss碰到连线时，重置所有触发器
/// </summary>
public class AttackTriggerManager : MonoBehaviour
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static AttackTriggerManager Instance { get; private set; }

    /// <summary>
    /// 连线材质
    /// </summary>
    [Tooltip("连线材质")]
    public Material lineMaterial;

    /// <summary>
    /// 连线宽度
    /// </summary>
    [Tooltip("连线宽度")]
    public float lineWidth = 0.1f;

    /// <summary>
    /// 连线颜色
    /// </summary>
    [Tooltip("连线颜色")]
    public Color lineColor = Color.yellow;

    /// <summary>
    /// 连线碰撞检测半径
    /// </summary>
    [Tooltip("连线碰撞检测半径")]
    public float collisionRadius = 0.5f;

    /// <summary>
    /// 所有已激活的触发器列表
    /// </summary>
    private List<AttackTrigger> activatedTriggers = new List<AttackTrigger>();

    /// <summary>
    /// 用于绘制连线的LineRenderer
    /// </summary>
    private LineRenderer lineRenderer;

    /// <summary>
    /// 用于碰撞检测的对象
    /// </summary>
    private GameObject collisionObject;

    /// <summary>
    /// 碰撞检测的SphereCollider
    /// </summary>
    private SphereCollider[] sphereColliders;

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 创建LineRenderer组件
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;

        // 创建碰撞检测对象
        CreateCollisionObject();
    }

    /// <summary>
    /// 创建碰撞检测对象
    /// </summary>
    void CreateCollisionObject()
    {
        collisionObject = new GameObject("AttackLineCollision");
        collisionObject.transform.SetParent(transform);
    }

    /// <summary>
    /// 更新碰撞检测
    /// </summary>
    void UpdateCollision()
    {
        // 先清除旧的碰撞检测
        ClearCollision();

        if (activatedTriggers.Count >= 2)
        {
            // 在连线上创建多个SphereCollider进行碰撞检测
            Vector3 start = activatedTriggers[0].transform.position;
            Vector3 end = activatedTriggers[1].transform.position;
            float distance = Vector3.Distance(start, end);
            
            // 计算需要多少个碰撞检测点（每隔0.5米一个）
            int colliderCount = Mathf.Max(2, Mathf.CeilToInt(distance / 0.5f));
            sphereColliders = new SphereCollider[colliderCount];

            for (int i = 0; i < colliderCount; i++)
            {
                float t = (float)i / (colliderCount - 1);
                Vector3 position = Vector3.Lerp(start, end, t);

                GameObject colliderObj = new GameObject($"Collider_{i}");
                colliderObj.transform.SetParent(collisionObject.transform);
                colliderObj.transform.position = position;

                SphereCollider collider = colliderObj.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.radius = collisionRadius;

                sphereColliders[i] = collider;

                // 添加碰撞检测脚本
                LineCollisionDetector detector = colliderObj.AddComponent<LineCollisionDetector>();
                detector.manager = this;
            }
        }
    }

    /// <summary>
    /// 清除碰撞检测
    /// </summary>
    void ClearCollision()
    {
        if (collisionObject != null)
        {
            foreach (Transform child in collisionObject.transform)
            {
                Destroy(child.gameObject);
            }
        }
        sphereColliders = null;
    }

    /// <summary>
    /// 当触发器激活时调用
    /// </summary>
    public void OnTriggerActivated(AttackTrigger trigger)
    {
        if (!activatedTriggers.Contains(trigger))
        {
            activatedTriggers.Add(trigger);
            Debug.Log("已激活触发器数量: " + activatedTriggers.Count);
        }

        // 检查是否有至少两个激活的触发器
        UpdateLine();
    }

    /// <summary>
    /// 更新连线
    /// </summary>
    void UpdateLine()
    {
        if (activatedTriggers.Count >= 2)
        {
            // 在第一个和第二个激活的触发器之间绘制连线
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, activatedTriggers[0].transform.position);
            lineRenderer.SetPosition(1, activatedTriggers[1].transform.position);
            lineRenderer.enabled = true;
            
            // 更新碰撞检测
            UpdateCollision();
            
            Debug.Log("连线已绘制在 " + activatedTriggers[0].gameObject.name + " 和 " + activatedTriggers[1].gameObject.name + " 之间");
        }
        else
        {
            lineRenderer.enabled = false;
            ClearCollision();
        }
    }

    void Update()
    {
        // 如果连线启用，持续更新位置（以防触发器移动）
        if (lineRenderer.enabled && activatedTriggers.Count >= 2)
        {
            lineRenderer.SetPosition(0, activatedTriggers[0].transform.position);
            lineRenderer.SetPosition(1, activatedTriggers[1].transform.position);
            
            // 更新碰撞检测位置
            if (sphereColliders != null && sphereColliders.Length >= 2)
            {
                Vector3 start = activatedTriggers[0].transform.position;
                Vector3 end = activatedTriggers[1].transform.position;
                
                for (int i = 0; i < sphereColliders.Length; i++)
                {
                    if (sphereColliders[i] != null)
                    {
                        float t = (float)i / (sphereColliders.Length - 1);
                        sphereColliders[i].transform.position = Vector3.Lerp(start, end, t);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 当检测到玩家或Boss碰到连线时调用
    /// </summary>
    public void OnLineHit()
    {
        Debug.Log("连线被触碰，重置所有触发器");
        ResetAllTriggers();
    }

    /// <summary>
    /// 重置所有触发器
    /// </summary>
    public void ResetAllTriggers()
    {
        foreach (var trigger in activatedTriggers)
        {
            trigger.ResetTrigger();
        }
        activatedTriggers.Clear();
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 0;
        ClearCollision();
        Debug.Log("所有触发器已重置");
    }

    /// <summary>
    /// 获取已激活的触发器数量
    /// </summary>
    public int GetActivatedCount()
    {
        return activatedTriggers.Count;
    }
}

/// <summary>
/// 连线碰撞检测脚本
/// </summary>
public class LineCollisionDetector : MonoBehaviour
{
    public AttackTriggerManager manager;

    void OnTriggerEnter(Collider other)
    {
        // 检测是否是玩家或Boss
        if (other.CompareTag("Player") || other.CompareTag("Boss"))
        {
            if (manager != null)
            {
                manager.OnLineHit();
            }
        }
    }
}
