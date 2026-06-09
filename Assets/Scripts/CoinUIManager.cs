using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CoinUIManager : MonoBehaviour
{
    public static CoinUIManager Instance;

    [Tooltip("显示金币数量的文本组件")]
    public TextMeshProUGUI counterText;

    private int coinCount = 0;

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

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        RefreshCounterTextReference();
        UpdateCounterText();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshCounterTextReference();
        UpdateCounterText();
    }

    private void RefreshCounterTextReference()
    {
        counterText = null;
        
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
        foreach (var text in allTexts)
        {
            if (text.name == "Counter")
            {
                counterText = text;
                Debug.Log("CoinUIManager: 自动找到Counter文本组件");
                return;
            }
        }
        
        Debug.LogWarning("CoinUIManager: 未找到名为'Counter'的TextMeshProUGUI组件");
    }

    public static void AddCoin()
    {
        if (Instance != null)
        {
            Instance.coinCount++;
            Instance.UpdateCounterText();
            Debug.Log($"金币+1，总数: {Instance.coinCount}");
        }
    }

    public static void ResetCoins()
    {
        if (Instance != null)
        {
            Instance.coinCount = 0;
            Instance.UpdateCounterText();
        }
    }

    public static void ResetCoinsAndRefreshUI()
    {
        if (Instance != null)
        {
            Instance.coinCount = 0;
            Instance.RefreshCounterTextReference();
            Instance.UpdateCounterText();
        }
    }

    private void UpdateCounterText()
    {
        if (counterText != null)
        {
            counterText.text = coinCount.ToString();
        }
        else
        {
            Debug.LogWarning("CoinUIManager: counterText引用为空！");
        }
    }

    public static int GetCoinCount()
    {
        return Instance != null ? Instance.coinCount : 0;
    }

    public static bool SubtractCoins(int amount)
    {
        if (Instance != null)
        {
            if (Instance.coinCount >= amount)
            {
                Instance.coinCount -= amount;
                Instance.UpdateCounterText();
                Debug.Log($"金币-{amount}，总数: {Instance.coinCount}");
                return true;
            }
            else
            {
                Debug.Log("金币不足！");
                return false;
            }
        }
        return false;
    }
}
