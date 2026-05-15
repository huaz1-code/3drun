using UnityEngine;

public class Platform : MonoBehaviour
{
    [Tooltip("材质列表（随机切换）")]
    public Material[] materials;
    
    [Tooltip("调试模式")]
    public bool debugMode = false;
    
    private Renderer platformRenderer;
    private GameObject currentCoin;
    private GameObject currentNPC;
    
    public bool HasNPC { get; set; }
    
    void Awake()
    {
        platformRenderer = GetComponent<Renderer>();
    }
    
    public void ResetPlatform()
    {
        if (debugMode) Debug.Log($"Platform.ResetPlatform: {name}, 金币: {currentCoin}, NPC: {currentNPC}");
        RecycleCoin();
        RecycleNPC();
        HasNPC = false;
        RandomizeMaterial();
    }
    
    void RandomizeMaterial()
    {
        if (platformRenderer != null && materials != null && materials.Length > 0)
        {
            int randomIndex = Random.Range(0, materials.Length);
            platformRenderer.material = materials[randomIndex];
        }
    }
    
    public bool HasCoin()
    {
        return currentCoin != null;
    }
    
    public void SetCoin(GameObject coin)
    {
        if (debugMode && coin != null) Debug.Log($"Platform.SetCoin: {name} 设置金币 {coin.name}");
        currentCoin = coin;
    }
    
    public void SetNPC(GameObject npc)
    {
        if (debugMode && npc != null) Debug.Log($"Platform.SetNPC: {name} 设置NPC {npc.name}");
        currentNPC = npc;
    }
    
    public void RecycleCoin()
    {
        if (currentCoin != null)
        {
            if (debugMode) Debug.Log($"Platform.RecycleCoin: {name} 回收金币 {currentCoin.name}");
            
            if (CoinPool.Instance != null)
            {
                CoinPool.Instance.ReturnCoin(currentCoin);
            }
            currentCoin = null;
        }
        else if (debugMode)
        {
            Debug.Log($"Platform.RecycleCoin: {name} 没有金币可回收");
        }
    }
    
    public void RecycleNPC()
    {
        if (currentNPC != null)
        {
            if (debugMode) Debug.Log($"Platform.RecycleNPC: {name} 回收NPC {currentNPC.name}");
            
            if (NPCPool.Instance != null)
            {
                NPCPool.Instance.ReturnNPC(currentNPC);
            }
            currentNPC = null;
        }
        HasNPC = false;
    }
    
    void OnDestroy()
    {
        RecycleCoin();
        RecycleNPC();
    }
}