using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 攻击触发器管理器 - 管理多个AttackTrigger之间的连线
/// 功能说明：
/// 1. 跟踪所有激活的AttackTrigger
/// 2. 当两个触发器激活时，在它们之间绘制连线（支持延时）
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
    /// 连线出现延时时间（秒）
    /// </summary>
    [Tooltip("两个触发器都激活后，连线出现的延时时间（秒）")]
    public float lineDelay = 2f;

    /// <summary>
    /// 连线伤害值
    /// </summary>
    [Tooltip("玩家或Boss碰到连线时受到的伤害值")]
    public float lineDamage = 20f;

    /// <summary>
    /// 是否在造成伤害后销毁连线
    /// </summary>
    [Tooltip("是否在造成伤害后销毁连线")]
    public bool destroyOnHit = true;

    /// <summary>
    /// 伤害冷却时间（秒）
    /// 防止同一目标在短时间内多次受到连线伤害
    /// </summary>
    [Tooltip("伤害冷却时间（秒）- 防止同一目标在短时间内多次受到连线伤害")]
    public float damageCooldown = 1f;

    /// <summary>
    /// 上一次受到伤害的目标名称
    /// </summary>
    private string lastHitTarget;

    /// <summary>
    /// 上一次造成伤害的时间
    /// </summary>
    private float lastHitTime;

    /// <summary>
    /// 伤害冷却字典：key为"targetName_lineIndex"，value为上次受伤时间
    /// 用于实现不同连线可以独立造成伤害
    /// </summary>
    private Dictionary<string, float> lineDamageCooldowns = new Dictionary<string, float>();

    /// <summary>
    /// 所有已激活的触发器列表
    /// </summary>
    private List<AttackTrigger> activatedTriggers = new List<AttackTrigger>();

    /// <summary>
    /// 用于绘制多条连线的LineRenderer列表
    /// </summary>
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();

    /// <summary>
    /// 用于碰撞检测的对象列表（每条连线一个）
    /// </summary>
    private List<GameObject> collisionObjects = new List<GameObject>();

    /// <summary>
    /// 当前正在运行的延时协程
    /// </summary>
    private Coroutine delayCoroutine;

    /// <summary>
    /// 已显示的连线数量（用于追踪已完成的配对）
    /// </summary>
    private int displayedLineCount = 0;

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
        // 初始化连线系统（不需要预先创建LineRenderer，会按需创建）
    }

    /// <summary>
    /// 创建一个新的LineRenderer（创建独立的GameObject）
    /// </summary>
    private LineRenderer CreateLineRenderer()
    {
        // 创建独立的GameObject来承载LineRenderer
        GameObject lineObj = new GameObject("AttackLine");
        lineObj.transform.SetParent(transform);
        
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = lineColor;
        lr.endColor = lineColor;
        lr.positionCount = 0;
        lr.enabled = false;
        return lr;
    }

    /// <summary>
    /// 创建碰撞检测对象
    /// </summary>
    private GameObject CreateCollisionObject(int lineIndex)
    {
        GameObject collisionObj = new GameObject($"AttackLineCollision_{lineIndex}");
        collisionObj.transform.SetParent(transform);
        return collisionObj;
    }

    /// <summary>
    /// 更新所有连线的碰撞检测
    /// </summary>
    void UpdateAllCollisions()
    {
        // 先清除旧的碰撞检测
        ClearAllCollisions();

        int lineCount = activatedTriggers.Count / 2;
        
        for (int lineIndex = 0; lineIndex < lineCount; lineIndex++)
        {
            AttackTrigger trigger1 = activatedTriggers[lineIndex * 2];
            AttackTrigger trigger2 = activatedTriggers[lineIndex * 2 + 1];
            
            Vector3 start = trigger1.GetLinePosition();
            Vector3 end = trigger2.GetLinePosition();
            float distance = Vector3.Distance(start, end);
            
            // 创建碰撞检测对象
            GameObject collisionObj = CreateCollisionObject(lineIndex);
            collisionObjects.Add(collisionObj);
            
            // 计算需要多少个碰撞检测点（每隔0.5米一个）
            int colliderCount = Mathf.Max(2, Mathf.CeilToInt(distance / 0.5f));

            for (int i = 0; i < colliderCount; i++)
            {
                float t = (float)i / (colliderCount - 1);
                Vector3 position = Vector3.Lerp(start, end, t);

                GameObject colliderObj = new GameObject($"Collider_{i}");
                colliderObj.transform.SetParent(collisionObj.transform);
                colliderObj.transform.position = position;

                SphereCollider collider = colliderObj.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.radius = collisionRadius;

                // 添加碰撞检测脚本
                LineCollisionDetector detector = colliderObj.AddComponent<LineCollisionDetector>();
                detector.manager = this;
                detector.lineIndex = lineIndex; // 设置连线索引
            }
        }
    }

    /// <summary>
    /// 更新指定连线的碰撞检测位置
    /// </summary>
    void UpdateCollisionPositions(int lineIndex)
    {
        // 检查索引是否有效
        if (lineIndex >= collisionObjects.Count || lineIndex >= activatedTriggers.Count / 2)
            return;
            
        int triggerIndex1 = lineIndex * 2;
        int triggerIndex2 = lineIndex * 2 + 1;
        
        // 检查触发器是否存在
        if (triggerIndex2 >= activatedTriggers.Count)
            return;
            
        AttackTrigger trigger1 = activatedTriggers[triggerIndex1];
        AttackTrigger trigger2 = activatedTriggers[triggerIndex2];
        
        if (trigger1 == null || trigger2 == null)
            return;
            
        Vector3 start = trigger1.GetLinePosition();
        Vector3 end = trigger2.GetLinePosition();
        
        GameObject collisionObj = collisionObjects[lineIndex];
        
        // 检查碰撞对象是否存在
        if (collisionObj == null)
            return;
            
        int childCount = collisionObj.transform.childCount;
        
        for (int i = 0; i < childCount; i++)
        {
            Transform child = collisionObj.transform.GetChild(i);
            if (child != null)
            {
                float t = (float)i / (childCount - 1);
                child.position = Vector3.Lerp(start, end, t);
            }
        }
    }

    /// <summary>
    /// 清除所有碰撞检测
    /// </summary>
    void ClearAllCollisions()
    {
        foreach (var collisionObj in collisionObjects)
        {
            if (collisionObj != null)
            {
                Destroy(collisionObj);
            }
        }
        collisionObjects.Clear();
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
        // 计算需要的连线数量（每两个触发器一条线）
        int requiredLineCount = activatedTriggers.Count / 2;
        
        // 如果当前需要显示的连线数少于已显示的，说明有触发器被重置
        if (requiredLineCount < displayedLineCount)
        {
            // 重新显示所有连线
            ShowLines();
            return;
        }
        
        // 计算新增的连线数量
        int newLineCount = requiredLineCount - displayedLineCount;
        
        // 如果有新增的配对
        if (newLineCount > 0)
        {
            // 如果有延时且当前没有正在运行的延时协程，才启动延时
            if (lineDelay > 0 && delayCoroutine == null)
            {
                // 启动延时协程
                delayCoroutine = StartCoroutine(ShowLinesWithDelay());
                Debug.Log("等待 " + lineDelay + " 秒后显示连线...");
            }
            else if (lineDelay == 0)
            {
                // 无延时，立即显示连线
                ShowLines();
            }
            // 如果有延时但已有协程在运行，不打断它，等待当前延时完成
        }
        else if (requiredLineCount == 0)
        {
            // 如果没有配对，隐藏所有连线
            if (delayCoroutine != null)
            {
                StopCoroutine(delayCoroutine);
                delayCoroutine = null;
            }
            
            HideAllLines();
            displayedLineCount = 0;
        }
        // 如果 requiredLineCount > 0 但 newLineCount == 0，说明没有新增配对，保持现状
    }

    /// <summary>
    /// 延时显示连线的协程
    /// </summary>
    IEnumerator ShowLinesWithDelay()
    {
        yield return new WaitForSeconds(lineDelay);
        
        ShowLines();
        delayCoroutine = null;
    }

    /// <summary>
    /// 显示所有连线
    /// </summary>
    void ShowLines()
    {
        // 计算完整配对的数量（每两个触发器形成一条线）
        int requiredLineCount = activatedTriggers.Count / 2;
        
        // 如果没有完整的配对，直接返回
        if (requiredLineCount < 1)
            return;
        
        // 确保有足够的LineRenderer
        while (lineRenderers.Count < requiredLineCount)
        {
            LineRenderer lr = CreateLineRenderer();
            lineRenderers.Add(lr);
        }
        
        // 隐藏多余的LineRenderer
        for (int i = requiredLineCount; i < lineRenderers.Count; i++)
        {
            if (lineRenderers[i] != null)
            {
                lineRenderers[i].enabled = false;
            }
        }
        
        // 绘制每条连线
        for (int i = 0; i < requiredLineCount; i++)
        {
            int triggerIndex1 = i * 2;
            int triggerIndex2 = i * 2 + 1;
            
            // 安全检查
            if (triggerIndex2 >= activatedTriggers.Count)
                break;
                
            AttackTrigger trigger1 = activatedTriggers[triggerIndex1];
            AttackTrigger trigger2 = activatedTriggers[triggerIndex2];
            
            if (trigger1 == null || trigger2 == null)
                continue;
                
            LineRenderer lr = lineRenderers[i];
            lr.positionCount = 2;
            lr.SetPosition(0, trigger1.GetLinePosition());
            lr.SetPosition(1, trigger2.GetLinePosition());
            lr.enabled = true;
            
            Debug.Log($"连线{i + 1}已绘制在 {trigger1.gameObject.name} 和 {trigger2.gameObject.name} 之间");
        }
        
        // 更新碰撞检测
        UpdateAllCollisions();
        
        // 更新已显示连线数量
        displayedLineCount = requiredLineCount;
    }

    /// <summary>
    /// 隐藏所有连线
    /// </summary>
    void HideAllLines()
    {
        foreach (var lr in lineRenderers)
        {
            if (lr != null)
            {
                lr.enabled = false;
            }
        }
        ClearAllCollisions();
    }

    void Update()
    {
        // 如果有连线启用，持续更新位置（以防触发器移动）
        int lineCount = lineRenderers.Count;
        
        for (int i = 0; i < lineCount; i++)
        {
            if (lineRenderers[i] != null && lineRenderers[i].enabled)
            {
                int triggerIndex1 = i * 2;
                int triggerIndex2 = i * 2 + 1;
                
                // 安全检查：确保触发器索引有效
                if (triggerIndex2 >= activatedTriggers.Count)
                    continue;
                    
                AttackTrigger trigger1 = activatedTriggers[triggerIndex1];
                AttackTrigger trigger2 = activatedTriggers[triggerIndex2];
                
                if (trigger1 == null || trigger2 == null)
                    continue;
                    
                lineRenderers[i].SetPosition(0, trigger1.GetLinePosition());
                lineRenderers[i].SetPosition(1, trigger2.GetLinePosition());
                
                // 更新碰撞检测位置
                UpdateCollisionPositions(i);
            }
        }
    }

    /// <summary>
    /// 当检测到玩家或Boss碰到连线时调用
    /// </summary>
    /// <param name="hitCollider">碰撞到连线的对象的Collider</param>
    /// <param name="lineIndex">被碰撞的连线索引（-1表示未知）</param>
    public void OnLineHit(Collider hitCollider, int lineIndex = -1)
    {
        string targetName = hitCollider.gameObject.name;
        float currentTime = Time.time;

        // 如果连线索引无效，直接返回
        if (lineIndex < 0)
            return;

        // 创建唯一的冷却key（目标名称 + 连线索引）
        string cooldownKey = targetName + "_" + lineIndex;

        // 检查该目标+该连线的冷却状态
        if (lineDamageCooldowns.ContainsKey(cooldownKey))
        {
            if (currentTime - lineDamageCooldowns[cooldownKey] < damageCooldown)
            {
                // 还在冷却中，不造成伤害
                return;
            }
        }

        // 更新该目标+该连线的冷却时间
        lineDamageCooldowns[cooldownKey] = currentTime;

        // 尝试获取Health组件
        Health health = hitCollider.GetComponent<Health>();
        if (health == null)
        {
            Debug.LogWarning(targetName + " 没有 Health 组件，无法造成伤害");
            return;
        }

        // 检查护盾
        PotionEffects potionEffects = health.GetComponent<PotionEffects>();
        if (potionEffects != null && potionEffects.CheckAndConsumeShield())
        {
            Debug.Log(targetName + " 碰到连线" + (lineIndex + 1) + "的伤害被护盾抵挡！");
            return;
        }

        // 直接对实际碰到线的对象造成伤害
        health.TakeDamage(lineDamage);
        Debug.Log(targetName + " 碰到连线" + (lineIndex + 1) + "，受到 " + lineDamage + " 点伤害");

        // 记录目标名称（用于调试）
        lastHitTarget = targetName;
        lastHitTime = currentTime;

        // 如果设置为碰撞后销毁连线
        if (destroyOnHit)
        {
            // 只重置被碰撞的那一对触发器
            Debug.Log($"连线{lineIndex + 1}被触碰，重置对应的触发器");
            ResetTriggerPair(lineIndex);
        }
    }

    /// <summary>
    /// 重置指定的一对触发器（只影响这一对，其他触发器不受影响）
    /// </summary>
    /// <param name="lineIndex">连线索引（从0开始）</param>
    public void ResetTriggerPair(int lineIndex)
    {
        int triggerIndex1 = lineIndex * 2;
        int triggerIndex2 = lineIndex * 2 + 1;
        
        // 检查索引是否有效
        if (triggerIndex2 >= activatedTriggers.Count)
            return;
            
        AttackTrigger trigger1 = activatedTriggers[triggerIndex1];
        AttackTrigger trigger2 = activatedTriggers[triggerIndex2];
        
        // 重置这两个触发器
        if (trigger1 != null)
        {
            trigger1.ResetTrigger();
            Debug.Log($"触发器 {trigger1.gameObject.name} 已重置");
        }
        if (trigger2 != null)
        {
            trigger2.ResetTrigger();
            Debug.Log($"触发器 {trigger2.gameObject.name} 已重置");
        }
        
        // 从激活列表中移除这两个触发器
        activatedTriggers.RemoveAt(triggerIndex2);
        activatedTriggers.RemoveAt(triggerIndex1);
        
        // 隐藏对应的连线
        if (lineIndex < lineRenderers.Count && lineRenderers[lineIndex] != null)
        {
            lineRenderers[lineIndex].enabled = false;
        }
        
        // 更新已显示连线数量
        displayedLineCount = activatedTriggers.Count / 2;
        
        // 更新碰撞检测
        UpdateAllCollisions();
        
        // 重新显示剩余的连线
        ShowLines();
    }

    /// <summary>
    /// 重置所有触发器
    /// </summary>
    public void ResetAllTriggers()
    {
        // 如果有延时协程在运行，停止它
        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
            delayCoroutine = null;
        }
        
        foreach (var trigger in activatedTriggers)
        {
            if (trigger != null)
            {
                trigger.ResetTrigger();
            }
        }
        activatedTriggers.Clear();
        
        // 隐藏所有连线
        HideAllLines();
        
        // 重置已显示连线数量
        displayedLineCount = 0;
        
        // 清空冷却记录
        lineDamageCooldowns.Clear();
        
        Debug.Log("所有触发器已重置");
    }

    /// <summary>
    /// 清理所有动态创建的对象
    /// </summary>
    void CleanupLineRenderers()
    {
        foreach (var lr in lineRenderers)
        {
            if (lr != null)
            {
                Destroy(lr.gameObject);
            }
        }
        lineRenderers.Clear();
    }

    /// <summary>
    /// 销毁时清理资源
    /// </summary>
    void OnDestroy()
    {
        CleanupLineRenderers();
        ClearAllCollisions();
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
    
    /// <summary>
    /// 所属的连线索引
    /// </summary>
    public int lineIndex;

    void OnTriggerEnter(Collider other)
    {
        // 检测是否是玩家或Boss
        if (other.CompareTag("Player") || other.CompareTag("Boss"))
        {
            if (manager != null)
            {
                manager.OnLineHit(other, lineIndex);
            }
        }
    }
}
