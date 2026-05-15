using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Tooltip("NPC在平台上方的偏移")]
    public float npcHeightOffset = 1.5f;
    
    [Tooltip("NPC生成间隔（每几个平台生成一个）")]
    public int spawnInterval = 10;
    
    [Tooltip("NPC开始生成的平台索引")]
    public int startSpawnIndex = 10;
    
    public void SpawnNPCOnPlatform(GameObject platform, int platformIndex)
    {
        if (platformIndex < startSpawnIndex || platformIndex % spawnInterval != 0)
            return;
        
        if (NPCPool.Instance == null) return;
        
        var renderer = platform.GetComponent<Renderer>();
        if (renderer == null) return;
        
        var bounds = renderer.bounds;
        
        float randomX = Random.Range(bounds.min.x + 1f, bounds.max.x - 1f);
        float randomZ = Random.Range(bounds.min.z + 1f, bounds.max.z - 1f);
        float y = bounds.max.y + npcHeightOffset;
        
        Vector3 spawnPosition = new Vector3(randomX, y, randomZ);
        
        GameObject npc = NPCPool.Instance.GetNPC(spawnPosition);
        
        if (npc != null)
        {
            Platform platformComp = platform.GetComponent<Platform>();
            if (platformComp != null)
            {
                platformComp.HasNPC = true;
                platformComp.SetNPC(npc);
            }
        }
    }
}