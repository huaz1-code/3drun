using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 攻击触发器管理器 - 管理多个AttackTrigger之间的连线
/// 功能说明：
/// 1. 跟踪所有激活的AttackTrigger
/// 2. 当两个触发器激活时，在它们之间绘制连线
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
    /// 所有已激活的触发器列表
    /// </summary>
    private List<AttackTrigger> activatedTriggers = new List<AttackTrigger>();

    /// <summary>
    /// 用于绘制连线的LineRenderer
    /// </summary>
    private LineRenderer lineRenderer;

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
            Debug.Log("连线已绘制在 " + activatedTriggers[0].gameObject.name + " 和 " + activatedTriggers[1].gameObject.name + " 之间");
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    void Update()
    {
        // 如果连线启用，持续更新位置（以防触发器移动）
        if (lineRenderer.enabled && activatedTriggers.Count >= 2)
        {
            lineRenderer.SetPosition(0, activatedTriggers[0].transform.position);
            lineRenderer.SetPosition(1, activatedTriggers[1].transform.position);
        }
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
