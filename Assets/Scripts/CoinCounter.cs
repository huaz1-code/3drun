using UnityEngine;

/// <summary>
/// 金币计数器脚本 - 管理金币收集和UI显示
/// 功能说明：
/// 1. 单例模式，提供全局访问的金币计数
/// 2. 静态方法AddCoin用于增加金币
/// 3. 在屏幕右上角显示金币数量
/// 4. 支持金币图标的显示
/// </summary>
public class CoinCounter : MonoBehaviour
{
    /// <summary>
    /// 单例实例 - 全局唯一的计数器引用
    /// 用于静态方法访问实例数据
    /// </summary>
    public static CoinCounter Instance;

    /// <summary>
    /// 当前金币数量 - 已收集的金币总数
    /// </summary>
    [Tooltip("金币数量")]
    public int coinCount = 0;

    /// <summary>
    /// 字体大小 - UI文字的大小
    /// </summary>
    [Tooltip("字体大小")]
    public int fontSize = 36;

    /// <summary>
    /// 文本颜色 - UI文字的颜色
    /// </summary>
    [Tooltip("文本颜色")]
    public Color textColor = Color.yellow;

    /// <summary>
    /// 显示偏移 - UI元素距离屏幕边缘的位置
    /// </summary>
    [Tooltip("显示位置（屏幕右上角）")]
    public Vector2 offset = new Vector2(20f, 20f);

    /// <summary>
    /// 金币图标 - 显示在数字旁边的图标纹理
    /// 可选，如果不设置则只显示数字
    /// </summary>
    [Tooltip("金币图标")]
    public Texture2D coinIcon;

    /// <summary>
    /// 图标大小 - 图标的像素大小
    /// </summary>
    [Tooltip("图标大小")]
    public int iconSize = 32;

    /// <summary>
    /// 唤醒方法 - 对象创建时调用
    /// 职责：实现单例模式，确保场景中只有一个实例
    /// </summary>
    void Awake()
    {
        // 检查是否已存在实例
        if (Instance == null)
        {
            // 第一次创建
            Instance = this;

            // 标记为不随场景切换而销毁
            // 确保在整个游戏过程中只有一个计数器
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 已经有实例存在，销毁当前对象
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 增加金币 - 静态方法，用于收集金币时调用
    /// 可以从任何脚本通过 CoinCounter.AddCoin() 调用
    /// </summary>
    public static void AddCoin()
    {
        // 检查实例是否存在
        if (Instance != null)
        {
            // 增加金币数量
            Instance.coinCount++;

            // 输出调试日志
            Debug.Log($"金币+1，总数: {Instance.coinCount}");
        }
    }

    /// <summary>
    /// 获取金币数量 - 静态方法，用于查询当前金币数
    /// </summary>
    /// <returns>当前收集的金币总数</returns>
    public static int GetCoinCount()
    {
        // 三元运算符：如果实例存在返回coinCount，否则返回0
        return Instance != null ? Instance.coinCount : 0;
    }

    /// <summary>
    /// 重置金币 - 静态方法，用于游戏重新开始时调用
    /// </summary>
    public static void ResetCoins()
    {
        if (Instance != null)
        {
            Instance.coinCount = 0;
        }
    }

    /// <summary>
    /// GUI绘制回调 - 每帧调用
    /// 职责：在屏幕上绘制金币UI
    /// </summary>
    void OnGUI()
    {
        // ========== 创建文字样式 ==========
        GUIStyle style = new GUIStyle();

        // 设置字体大小
        style.fontSize = fontSize;

        // 设置文字颜色
        style.normal.textColor = textColor;

        // 设置文字对齐方式：右对齐
        style.alignment = TextAnchor.MiddleRight;

        // ========== 计算UI元素位置 ==========
        // 使用屏幕分辨率计算位置，支持不同分辨率
        float x = Screen.width - offset.x;  // 右边缘 - 偏移
        float y = offset.y;                  // 上边缘 + 偏移

        // ========== 绘制金币图标 ==========
        if (coinIcon != null)
        {
            // 计算图标位置：在数字左侧
            float iconX = x - iconSize - 5 - fontSize * 2;
            float iconY = y;

            // 绘制图标纹理
            GUI.DrawTexture(new Rect(iconX, iconY, iconSize, iconSize), coinIcon);
        }

        // ========== 绘制金币数量 ==========
        // 位置：屏幕右侧
        float labelWidth = fontSize * 3;
        float labelHeight = fontSize;
        float labelX = x - labelWidth;
        float labelY = y;

        // 使用Label显示数字
        GUI.Label(new Rect(labelX, labelY, labelWidth, labelHeight), coinCount.ToString(), style);
    }
}
