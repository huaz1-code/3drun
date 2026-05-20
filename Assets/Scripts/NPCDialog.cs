using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// NPC对话脚本 - 处理玩家与NPC的对话交互
/// 功能说明：
/// 1. 检测玩家进入交互范围
/// 2. 在NPC头上显示按E对话提示
/// 3. 按E键触发对话
/// 4. 显示对话内容，包含商店按钮
/// </summary>
public class NPCDialog : MonoBehaviour
{
    /// <summary>
    /// 对话面板 - 显示对话内容的UI面板
    /// </summary>
    [Tooltip("对话面板")]
    public GameObject dialogPanel;

    /// <summary>
    /// 说话人名字文本 - 显示NPC名字
    /// </summary>
    [Tooltip("说话人名字文本")]
    public TextMeshProUGUI speakerNameText;

    /// <summary>
    /// 对话内容文本 - 显示对话内容
    /// </summary>
    [Tooltip("对话内容文本")]
    public TextMeshProUGUI dialogText;

    /// <summary>
    /// 头上提示文本 - 在NPC头上显示"按E对话"提示
    /// </summary>
    [Tooltip("头上提示文本")]
    public TextMeshProUGUI overheadText;

    /// <summary>
    /// 提示高度偏移 - 提示相对于NPC头顶的高度
    /// </summary>
    [Tooltip("提示高度偏移")]
    public float textHeightOffset = 2f;

    /// <summary>
    /// 商店按钮 - 点击进入商店
    /// </summary>
    [Tooltip("商店按钮")]
    public Button shopButton;

    /// <summary>
    /// 商店面板 - 商店UI面板
    /// </summary>
    [Tooltip("商店面板")]
    public GameObject shopPanel;

    /// <summary>
    /// NPC名字 - 在对话中显示的名字
    /// </summary>
    [Tooltip("NPC名字")]
    public string npcName = "NPC";

    /// <summary>
    /// 对话内容 - NPC要说的话
    /// </summary>
    [Tooltip("对话内容")]
    public string dialogContent = "欢迎来到这个世界！";

    /// <summary>
    /// 玩家是否在交互范围内
    /// </summary>
    private bool playerInRange = false;

    /// <summary>
    /// 是否正在对话中
    /// </summary>
    private bool isTalking = false;

    /// <summary>
    /// 玩家控制器引用 - 用于暂停/恢复玩家移动
    /// </summary>
    private PlayerController playerController;

    /// <summary>
    /// 摄像机控制器引用 - 用于暂停/恢复摄像机旋转
    /// </summary>
    private CameraController cameraController;

    /// <summary>
    /// 提示文本的RectTransform - 用于设置位置
    /// </summary>
    private RectTransform overheadRectTransform;

    /// <summary>
    /// 初始化方法
    /// </summary>
    void Start()
    {
        // 隐藏对话面板
        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        // 隐藏商店面板
        if (shopPanel != null)
            shopPanel.SetActive(false);

        // 隐藏头上提示
        if (overheadText != null)
        {
            overheadText.gameObject.SetActive(false);
            overheadRectTransform = overheadText.GetComponent<RectTransform>();
        }

        // 设置商店按钮点击事件
        if (shopButton != null)
        {
            shopButton.onClick.AddListener(OpenShop);
        }

        // 设置NPC名字
        if (speakerNameText != null)
            speakerNameText.text = npcName;
    }

    /// <summary>
    /// 更新方法 - 检测E键输入和更新提示位置
    /// </summary>
    void Update()
    {
        // 更新头上提示的位置
        UpdateOverheadTextPosition();

        // 如果玩家在范围内且按E键
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isTalking)
            {
                // 开始对话
                StartDialog();
            }
            else
            {
                // 检查是否在商店界面，如果是则关闭商店，否则关闭对话
                if (shopPanel != null && shopPanel.activeSelf)
                {
                    CloseShop();
                }
                else
                {
                    EndDialog();
                }
            }
        }

        // 如果不在范围内且正在对话，结束对话
        if (!playerInRange && isTalking)
        {
            EndDialog();
        }
    }

    /// <summary>
    /// 更新头上提示的位置 - 让提示始终跟随NPC并面向摄像机
    /// </summary>
    void UpdateOverheadTextPosition()
    {
        if (overheadText == null || overheadRectTransform == null || Camera.main == null)
            return;

        // 计算NPC头顶位置（世界坐标）
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
        // 检查是否是玩家
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            // 获取玩家控制器引用
            playerController = other.GetComponent<PlayerController>();

            // 获取摄像机控制器引用
            if (cameraController == null)
            {
                cameraController = Camera.main?.GetComponent<CameraController>();
            }

            // 显示头上提示
            if (overheadText != null)
                overheadText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 玩家离开触发区域
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        // 检查是否是玩家
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            // 隐藏头上提示
            if (overheadText != null)
                overheadText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 开始对话
    /// </summary>
    void StartDialog()
    {
        isTalking = true;

        // 隐藏头上提示
        if (overheadText != null)
            overheadText.gameObject.SetActive(false);

        // 显示对话面板
        if (dialogPanel != null)
            dialogPanel.SetActive(true);

        // 设置对话内容
        if (dialogText != null)
            dialogText.text = dialogContent;

        // 暂停玩家移动
        if (playerController != null)
            playerController.enabled = false;

        // 立即停止玩家的物理速度，防止继续移动
        Rigidbody playerRb = playerController != null ? playerController.GetComponent<Rigidbody>() : null;
        if (playerRb != null)
        {
            playerRb.velocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        // 禁用摄像机控制器，防止对话时视角跟着鼠标转动
        if (cameraController == null && playerController != null)
        {
            cameraController = Camera.main?.GetComponent<CameraController>();
        }
        if (cameraController != null)
        {
            cameraController.enabled = false;
        }

        // 显示鼠标并解锁，以便点击UI按钮
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    void EndDialog()
    {
        isTalking = false;

        // 隐藏对话面板
        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        // 如果玩家还在范围内，显示头上提示
        if (playerInRange && overheadText != null)
            overheadText.gameObject.SetActive(true);

        // 恢复玩家移动
        if (playerController != null)
            playerController.enabled = true;

        // 重新启用摄像机控制器，恢复游戏状态
        if (cameraController != null)
        {
            cameraController.enabled = true;
        }

        // 隐藏鼠标并锁定，恢复游戏状态
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// 打开商店
    /// </summary>
    public void OpenShop()
    {
        // 隐藏对话面板
        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        // 显示商店面板
        if (shopPanel != null)
            shopPanel.SetActive(true);

        Debug.Log("打开商店");
    }

    /// <summary>
    /// 关闭商店
    /// </summary>
    public void CloseShop()
    {
        // 隐藏商店面板
        if (shopPanel != null)
            shopPanel.SetActive(false);

        // 显示对话面板
        if (dialogPanel != null)
            dialogPanel.SetActive(true);

        Debug.Log("关闭商店");
    }
}