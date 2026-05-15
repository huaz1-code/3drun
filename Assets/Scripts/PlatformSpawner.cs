using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Tooltip("玩家引用")]
    public Transform player;
    
    [Tooltip("前方保持的平台数量")]
    public int platformCountAhead = 8;
    
    [Tooltip("平台最小X位置")]
    public float minX = -5f;
    
    [Tooltip("平台最大X位置")]
    public float maxX = 5f;
    
    [Tooltip("平台最小Y位置")]
    public float minY = 0f;
    
    [Tooltip("平台最大Y位置")]
    public float maxY = 4f;
    
    [Tooltip("平台Z轴间距")]
    public float zSpacing = 8f;
    
    [Tooltip("平台宽度")]
    public float scaleX = 5f;
    
    [Tooltip("平台高度")]
    public float scaleY = 0.5f;
    
    [Tooltip("平台深度")]
    public float scaleZ = 3f;
    
    [Tooltip("回收距离（玩家后方）")]
    public float recycleDistance = 15f;
    
    [Tooltip("第一个平台距离玩家的偏移（Z轴）")]
    public float firstPlatformOffset = 5f;
    
    [Tooltip("调试模式")]
    public bool debugMode = true;
    
    private float nextSpawnZ;
    private float lastPlayerZ;
    private bool initialized = false;
    private int platformIndex = 0;
    
    void Start()
    {
        if (debugMode) Debug.Log("PlatformSpawner: 启动");
        
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                if (debugMode) Debug.Log("PlatformSpawner: 找到玩家 " + player.name);
            }
            else
            {
                if (debugMode) Debug.LogError("PlatformSpawner: 未找到玩家对象！请确保场景中有名为'player'的对象");
                enabled = false;
                return;
            }
        }
        
        if (PlatformPool.Instance == null)
        {
            if (debugMode) Debug.LogError("PlatformSpawner: 未找到PlatformPool！请确保场景中有PlatformPool对象");
            enabled = false;
            return;
        }
        
        lastPlayerZ = player.position.z;
        nextSpawnZ = player.position.z + firstPlatformOffset;
        
        if (debugMode) Debug.Log($"PlatformSpawner: 玩家位置 Z={lastPlayerZ}，第一个平台从 Z={nextSpawnZ} 开始");
        
        for (int i = 0; i < platformCountAhead; i++)
        {
            SpawnPlatform();
        }
        
        initialized = true;
        if (debugMode) Debug.Log($"PlatformSpawner: 初始化完成，生成{platformCountAhead}个平台");
    }
    
    void Update()
    {
        if (!initialized || player == null) return;
        
        float playerZ = player.position.z;
        
        if (playerZ > lastPlayerZ)
        {
            while (nextSpawnZ < playerZ + platformCountAhead * zSpacing)
            {
                SpawnPlatform();
            }
            
            RecycleOldPlatforms(playerZ);
        }
        
        lastPlayerZ = playerZ;
    }
    
    void SpawnPlatform()
    {
        if (PlatformPool.Instance == null)
        {
            if (debugMode) Debug.LogError("SpawnPlatform: PlatformPool.Instance为null");
            return;
        }
        
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        
        Vector3 position = new Vector3(randomX, randomY, nextSpawnZ);
        Vector3 scale = new Vector3(scaleX, scaleY, scaleZ);
        
        GameObject platform = PlatformPool.Instance.GetPlatform(position, scale);
        
        if (platform != null)
        {
            if (debugMode) Debug.Log($"SpawnPlatform: 生成平台 #{platformIndex} at Z={nextSpawnZ}");
            
            NPCSpawner npcSpawner = FindObjectOfType<NPCSpawner>();
            if (npcSpawner != null)
            {
                npcSpawner.SpawnNPCOnPlatform(platform, platformIndex);
            }
            
            CoinSpawner coinSpawner = FindObjectOfType<CoinSpawner>();
            if (coinSpawner != null)
            {
                coinSpawner.SpawnCoinsOnNewPlatform(platform);
            }
            
            platformIndex++;
        }
        else
        {
            if (debugMode) Debug.LogWarning("SpawnPlatform: 未能获取平台，池可能已满");
        }
        
        nextSpawnZ += zSpacing;
    }
    
    void RecycleOldPlatforms(float playerZ)
    {
        if (PlatformPool.Instance == null) return;
        
        for (int i = PlatformPool.Instance.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = PlatformPool.Instance.transform.GetChild(i);
            if (child.gameObject.activeSelf && child.position.z < playerZ - recycleDistance)
            {
                if (debugMode) Debug.Log($"RecycleOldPlatforms: 回收平台 at Z={child.position.z}");
                PlatformPool.Instance.ReturnPlatform(child.gameObject);
            }
        }
    }
}