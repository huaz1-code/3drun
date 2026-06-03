using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 攻击触发器脚本 - 处理玩家与攻击点的交互
/// 功能说明：
/// 1. 检测玩家靠近，显示按E提示
/// 2. 按E激活触发器
/// 3. 当两个触发器都激活时，它们之间会连线
/// </summary>
public class AttackTrigger : MonoBehaviour
{
    /// <summary>
    /// 提示高度偏移 - 提示相对于触发器的高度
    /// </summary>
    [Tooltip("提示高度偏移")]
    public float textHeightOffset = 2f;

    /// <summary>
    /// 提示文字大小
    /// </summary>
    [Tooltip("提示文字大小")]
    public int fontSize = 24;

    /// <summary>
    /// 提示文字内容
    /// </summary>
    [Tooltip("提示文字内容")]
    public string promptText = "按E激活";

    /// <summary>
    /// 提示文字颜色
    /// </summary>
    [Tooltip("提示文字颜色")]
    public Color textColor = Color.white;

    /// <summary>
    /// 提示文字字体
    /// </summary>
    [Tooltip("提示文字字体")]
    public TMP_FontAsset fontAsset;

    /// <summary>
    /// 触发器视觉标记 - 用于显示激活状态的对象
    /// </summary>
    [Tooltip("触发器视觉标记")]
    public GameObject visualMarker;

    /// <summary>
    /// 未激活时的颜色
    /// </summary>
    [Tooltip("未激活时的颜色")]
    public Color inactiveColor = Color.red;

    /// <summary>
    /// 激活时的颜色
    /// </summary>
    [Tooltip("激活时的颜色")]
    public Color activeColor = Color.green;

    /// <summary>
    /// 连线点位置类型
    /// </summary>
    [Tooltip("连线点的位置类型")]
    public PositionType linePositionType = PositionType.Center;

    /// <summary>
    /// 自定义偏移位置（当选择CustomOffset时使用）
    /// </summary>
    [Tooltip("自定义偏移位置（当选择CustomOffset时使用）")]
    public Vector3 customOffset = Vector3.zero;

    /// <summary>
    /// 位置类型枚举
    /// </summary>
    public enum PositionType
    {
        /// <summary>
        /// 使用对象原点位置（transform.position）
        /// </summary>
        Origin,
        /// <summary>
        /// 使用对象中心位置（Renderer bounds中心）
        /// </summary>
        Center,
        /// <summary>
        /// 使用自定义偏移位置
        /// </summary>
        CustomOffset
    }

    /// <summary>
    /// 是否已经激活
    /// </summary>
    private bool isActivated = false;

    /// <summary>
    /// 玩家是否在交互范围内
    /// </summary>
    private bool playerInRange = false;

    /// <summary>
    /// 提示文本组件（自动创建）
    /// </summary>
    private TextMeshProUGUI overheadText;

    /// <summary>
    /// 提示文本的RectTransform
    /// </summary>
    private RectTransform overheadRectTransform;

    /// <summary>
    /// 视觉标记的渲染器
    /// </summary>
    private Renderer markerRenderer;

    /// <summary>
    /// 静态Canvas引用（所有触发器共享）
    /// </summary>
    private static Canvas sharedCanvas;

    void Start()
    {
        // 自动创建提示文本
        CreateOverheadText();

        // 初始化视觉标记
        if (visualMarker != null)
        {
            markerRenderer = visualMarker.GetComponent<Renderer>();
            UpdateMarkerColor();
        }
        else
        {
            // 如果没有指定视觉标记，使用自身的渲染器
            markerRenderer = GetComponent<Renderer>();
            UpdateMarkerColor();
        }
    }

    /// <summary>
    /// 自动创建头上提示文本
    /// </summary>
    void CreateOverheadText()
    {
        // 确保Canvas存在
        EnsureCanvasExists();

        // 创建文本对象
        GameObject textObject = new GameObject("OverheadText_" + gameObject.name);
        textObject.transform.SetParent(sharedCanvas.transform);

        // 添加RectTransform组件
        overheadRectTransform = textObject.AddComponent<RectTransform>();
        overheadRectTransform.sizeDelta = new Vector2(200, 50);

        // 添加TextMeshProUGUI组件
        overheadText = textObject.AddComponent<TextMeshProUGUI>();
        overheadText.text = promptText;
        overheadText.fontSize = fontSize;
        overheadText.color = textColor;
        overheadText.alignment = TextAlignmentOptions.Center;
        
        // 使用用户指定的字体，如果没有指定则使用默认字体
        if (fontAsset != null)
        {
            overheadText.font = fontAsset;
        }
        else
        {
            // 尝试加载项目中的默认字体
            TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (defaultFont != null)
            {
                overheadText.font = defaultFont;
            }
        }

        // 隐藏文本
        textObject.SetActive(false);
    }

    /// <summary>
    /// 确保Canvas存在
    /// </summary>
    void EnsureCanvasExists()
    {
        if (sharedCanvas != null) return;

        // 查找现有Canvas
        sharedCanvas = FindObjectOfType<Canvas>();

        // 如果没有Canvas，创建一个新的
        if (sharedCanvas == null)
        {
            GameObject canvasObject = new GameObject("AttackTriggerCanvas");
            sharedCanvas = canvasObject.AddComponent<Canvas>();
            sharedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
    }

    void Update()
    {
        // 更新头上提示的位置
        UpdateOverheadTextPosition();

        // 如果玩家在范围内、未激活且按E键
        if (playerInRange && !isActivated && Input.GetKeyDown(KeyCode.E))
        {
            Activate();
        }
    }

    /// <summary>
    /// 更新头上提示的位置
    /// </summary>
    void UpdateOverheadTextPosition()
    {
        if (overheadText == null || overheadRectTransform == null || Camera.main == null)
            return;

        // 计算触发器上方位置（世界坐标）
        Vector3 worldPosition = transform.position + Vector3.up * textHeightOffset;

        // 转换为屏幕坐标
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        // 设置UI位置
        overheadRectTransform.position = screenPosition;

        // 确保提示始终在屏幕内
        Vector2 minPosition = new Vector2(overheadRectTransform.rect.width / 2, overheadRectTransform.rect.height / 2);
        Vector2 maxPosition = new Vector2(Screen.width - overheadRectTransform.rect.width / 2, Screen.height - overheadRectTransform.rect.height / 2);
        overheadRectTransform.position = new Vector2(
            Mathf.Clamp(screenPosition.x, minPosition.x, maxPosition.x),
            Mathf.Clamp(screenPosition.y, minPosition.y, maxPosition.y)
        );
    }

    /// <summary>
    /// 玩家进入触发区域
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // 如果未激活，显示头上提示
            if (!isActivated && overheadText != null)
                overheadText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 玩家离开触发区域
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // 隐藏头上提示
            if (overheadText != null)
                overheadText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 激活触发器
    /// </summary>
    void Activate()
    {
        isActivated = true;

        // 隐藏头上提示
        if (overheadText != null)
            overheadText.gameObject.SetActive(false);

        // 更新视觉标记颜色
        UpdateMarkerColor();

        // 通知AttackTriggerManager
        AttackTriggerManager.Instance?.OnTriggerActivated(this);

        Debug.Log("AttackTrigger 已激活: " + gameObject.name);
    }

    /// <summary>
    /// 更新视觉标记颜色
    /// </summary>
    void UpdateMarkerColor()
    {
        if (markerRenderer != null)
        {
            markerRenderer.material.color = isActivated ? activeColor : inactiveColor;
        }
    }

    /// <summary>
    /// 获取激活状态
    /// </summary>
    public bool IsActivated()
    {
        return isActivated;
    }

    /// <summary>
    /// 获取连线点的位置
    /// 根据配置的linePositionType返回对应的位置
    /// </summary>
    /// <returns>连线点的世界坐标位置</returns>
    public Vector3 GetLinePosition()
    {
        switch (linePositionType)
        {
            case PositionType.Origin:
                return transform.position;
            
            case PositionType.Center:
                return GetRendererCenter();
            
            case PositionType.CustomOffset:
                return transform.position + customOffset;
            
            default:
                return transform.position;
        }
    }

    /// <summary>
    /// 获取渲染器的中心位置
    /// 如果有visualMarker则使用它的中心，否则使用自身Renderer的中心
    /// </summary>
    /// <returns>Renderer的中心世界坐标</returns>
    private Vector3 GetRendererCenter()
    {
        Renderer targetRenderer = null;
        
        if (visualMarker != null)
        {
            targetRenderer = visualMarker.GetComponent<Renderer>();
        }
        
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }
        
        if (targetRenderer != null)
        {
            return targetRenderer.bounds.center;
        }
        
        return transform.position;
    }

    /// <summary>
    /// 重置触发器
    /// </summary>
    public void ResetTrigger()
    {
        isActivated = false;
        UpdateMarkerColor();
    }

    /// <summary>
    /// 清理方法
    /// </summary>
    void OnDestroy()
    {
        // 销毁自动创建的文本对象
        if (overheadText != null)
        {
            Destroy(overheadText.gameObject);
        }
    }
}
