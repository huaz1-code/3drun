using UnityEngine;
using System.Collections.Generic;

public class NPCPool : MonoBehaviour
{
    public static NPCPool Instance;
    
    [Tooltip("NPC预制体")]
    public GameObject npcPrefab;
    
    [Tooltip("初始池子大小")]
    public int initialPoolSize = 5;
    
    [Tooltip("最大池子大小")]
    public int maxPoolSize = 15;
    
    private Queue<GameObject> npcPool = new Queue<GameObject>();
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        InitializePool();
    }
    
    void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject npc = Instantiate(npcPrefab, transform);
            npc.SetActive(false);
            npcPool.Enqueue(npc);
        }
    }
    
    public GameObject GetNPC(Vector3 position)
    {
        GameObject npc = null;
        
        if (npcPool.Count > 0)
        {
            npc = npcPool.Dequeue();
        }
        else if (transform.childCount < maxPoolSize)
        {
            npc = Instantiate(npcPrefab, transform);
        }
        
        if (npc != null)
        {
            npc.SetActive(true);
            npc.transform.position = position;
            
            NPC npcComp = npc.GetComponent<NPC>();
            if (npcComp != null)
            {
                npcComp.ResetNPC();
            }
        }
        
        return npc;
    }
    
    public void ReturnNPC(GameObject npc)
    {
        if (npc == null) return;
        
        npc.SetActive(false);
        npcPool.Enqueue(npc);
    }
}