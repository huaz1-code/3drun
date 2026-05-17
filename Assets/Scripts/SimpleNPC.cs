using UnityEngine;

/// <summary>
/// 简单NPC脚本 - 第二个平台专属NPC
/// 功能说明：
/// 1. 在第二个平台生成
/// 2. 保持静止不动（不再跟随玩家）
/// 3. 平台消失时自动销毁
/// 4. 通过触发器检测玩家靠近，触发对话
/// </summary>
public class SimpleNPC : MonoBehaviour
{
    /// <summary>
    /// 初始化方法 - 游戏开始时调用
    /// </summary>
    void Start()
    {
        // NPC保持静止，不需要跟随玩家
    }

    /// <summary>
    /// 更新方法 - 每帧调用
    /// </summary>
    void Update()
    {
        // NPC保持静止，不需要任何移动逻辑
    }
}