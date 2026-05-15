using UnityEngine;
using System.Collections.Generic;

public class PlatformPool : MonoBehaviour
{
    public static PlatformPool Instance;
    
    [Tooltip("平台预制体")]
    public GameObject platformPrefab;
    
    [Tooltip("初始池子大小")]
    public int initialPoolSize = 15;
    
    [Tooltip("最大池子大小")]
    public int maxPoolSize = 30;
    
    [Tooltip("调试模式")]
    public bool debugMode = true;
    
    private Queue<GameObject> platformPool = new Queue<GameObject>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (debugMode) Debug.Log("PlatformPool: 单例创建成功");
        }
        else if (Instance != this)
        {
            if (debugMode) Debug.LogWarning("PlatformPool: 已存在实例，销毁重复对象");
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        if (platformPrefab == null)
        {
            if (debugMode) Debug.LogError("PlatformPool: platformPrefab未设置！请在Inspector中指定平台预制体");
            enabled = false;
            return;
        }
        
        InitializePool();
    }
    
    void InitializePool()
    {
        if (debugMode) Debug.Log($"PlatformPool: 开始初始化 {initialPoolSize} 个平台");
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject platform = Instantiate(platformPrefab, transform);
            platform.name = "Platform_" + (i + 1);
            platform.SetActive(false);
            platformPool.Enqueue(platform);
        }
        
        if (debugMode) Debug.Log($"PlatformPool: 初始化完成，池中共有 {platformPool.Count} 个平台");
    }
    
    public GameObject GetPlatform(Vector3 position, Vector3 scale)
    {
        GameObject platform = null;
        
        if (platformPool.Count > 0)
        {
            platform = platformPool.Dequeue();
        }
        else if (transform.childCount < maxPoolSize)
        {
            if (debugMode) Debug.LogWarning("PlatformPool: 池已空，动态创建新平台");
            platform = Instantiate(platformPrefab, transform);
            platform.name = "Platform_" + (transform.childCount);
        }
        else
        {
            if (debugMode) Debug.LogError("PlatformPool: 达到最大数量，无法获取平台");
            return null;
        }
        
        if (platform != null)
        {
            platform.SetActive(true);
            platform.transform.position = position;
            platform.transform.localScale = scale;
            
            Platform platformComp = platform.GetComponent<Platform>();
            if (platformComp != null)
            {
                platformComp.ResetPlatform();
            }
        }
        
        return platform;
    }
    
    public void ReturnPlatform(GameObject platform)
    {
        if (platform == null) return;
        
        Platform platformComp = platform.GetComponent<Platform>();
        if (platformComp != null)
        {
            platformComp.ResetPlatform();
        }
        
        platform.SetActive(false);
        platformPool.Enqueue(platform);
    }
}