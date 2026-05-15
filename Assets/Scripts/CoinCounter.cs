using UnityEngine;

public class CoinCounter : MonoBehaviour
{
    public static CoinCounter Instance;
    
    [Tooltip("金币数量")]
    public int coinCount = 0;
    
    [Tooltip("字体大小")]
    public int fontSize = 36;
    
    [Tooltip("文本颜色")]
    public Color textColor = Color.yellow;
    
    [Tooltip("显示位置（屏幕右上角）")]
    public Vector2 offset = new Vector2(20f, 20f);
    
    [Tooltip("金币图标")]
    public Texture2D coinIcon;
    
    [Tooltip("图标大小")]
    public int iconSize = 32;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static void AddCoin()
    {
        if (Instance != null)
        {
            Instance.coinCount++;
            Debug.Log($"金币+1，总数: {Instance.coinCount}");
        }
    }
    
    public static int GetCoinCount()
    {
        return Instance != null ? Instance.coinCount : 0;
    }
    
    public static void ResetCoins()
    {
        if (Instance != null)
        {
            Instance.coinCount = 0;
        }
    }
    
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = fontSize;
        style.normal.textColor = textColor;
        style.alignment = TextAnchor.MiddleRight;
        
        float x = Screen.width - offset.x;
        float y = offset.y;
        
        if (coinIcon != null)
        {
            GUI.DrawTexture(new Rect(x - iconSize - 5 - fontSize * 2, y, iconSize, iconSize), coinIcon);
        }
        
        GUI.Label(new Rect(x - fontSize * 3, y, fontSize * 3, fontSize), coinCount.ToString(), style);
    }
}