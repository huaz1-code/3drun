using UnityEngine;
using TMPro;

/// <summary>
/// NPC对话脚本 - 处理玩家与NPC的对话交互
/// 功能说明：
/// 1. 检测玩家进入交互范围
/// 2. 显示/隐藏对话提示
/// 3. 按E键触发对话
/// 4. 显示对话内容
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
    /// 按E提示文本 - 显示"按E对话"提示
    /// </summary>
    [Tooltip("按E提示文本")]
    public TextMeshProUGUI pressEText;

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
    /// 初始化方法
    /// </summary>
    void Start()
    {
        // 隐藏对话面板和提示
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
        
        if (pressEText != null)
            pressEText.gameObject.SetActive(false);

        // 设置NPC名字
        if (speakerNameText != null)
            speakerNameText.text = npcName;
    }

    /// <summary>
    /// 更新方法 - 检测E键输入
    /// </summary>
    void Update()
    {
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
                // 结束对话
                EndDialog();
            }
        }

        // 如果不在范围内且正在对话，结束对话
        if (!playerInRange && isTalking)
        {
            EndDialog();
        }
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

            // 显示按E提示
            if (pressEText != null)
                pressEText.gameObject.SetActive(true);
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
            
            // 隐藏按E提示
            if (pressEText != null)
                pressEText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 开始对话
    /// </summary>
    void StartDialog()
    {
        isTalking = true;

        // 隐藏按E提示
        if (pressEText != null)
            pressEText.gameObject.SetActive(false);

        // 显示对话面板
        if (dialogPanel != null)
            dialogPanel.SetActive(true);

        // 设置对话内容
        if (dialogText != null)
            dialogText.text = dialogContent;

        // 暂停玩家移动
        if (playerController != null)
            playerController.enabled = false;
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

        // 如果玩家还在范围内，显示按E提示
        if (playerInRange && pressEText != null)
            pressEText.gameObject.SetActive(true);

        // 恢复玩家移动
        if (playerController != null)
            playerController.enabled = true;
    }
}