using UnityEngine;
using System.Collections.Generic;

public class CoinSpawner : MonoBehaviour
{
    [Tooltip("每个平台生成金币的概率（0-1）")]
    public float coinSpawnChance = 0.6f;
    
    [Tooltip("金币在平台上方的偏移")]
    public float coinHeightOffset = 1f;
    
    [Tooltip("金币生成的边缘留空")]
    public float edgePadding = 0.5f;
    
    void Start()
    {
        SpawnCoinsOnExistingPlatforms();
    }
    
    void SpawnCoinsOnExistingPlatforms()
    {
        var platforms = FindAllPlatforms();
        
        foreach (var platform in platforms)
        {
            Platform platformComp = platform.GetComponent<Platform>();
            if (platformComp != null && !platformComp.HasCoin() && !platformComp.HasNPC)
            {
                if (Random.value < coinSpawnChance)
                {
                    SpawnSingleCoinOnPlatform(platform);
                }
            }
        }
    }
    
    public void SpawnCoinsOnNewPlatform(GameObject platform)
    {
        if (platform == null) return;
        
        Platform platformComp = platform.GetComponent<Platform>();
        if (platformComp == null || platformComp.HasCoin() || platformComp.HasNPC)
        {
            return;
        }
        
        if (Random.value < coinSpawnChance)
        {
            SpawnSingleCoinOnPlatform(platform);
        }
    }
    
    List<GameObject> FindAllPlatforms()
    {
        var allObjects = FindObjectsOfType<GameObject>();
        var platforms = new List<GameObject>();
        
        foreach (var obj in allObjects)
        {
            if (obj.name.StartsWith("Platform_") && obj.activeSelf)
            {
                platforms.Add(obj);
            }
        }
        
        return platforms;
    }
    
    void SpawnSingleCoinOnPlatform(GameObject platform)
    {
        Platform platformComp = platform.GetComponent<Platform>();
        if (platformComp == null) return;
        
        if (platformComp.HasCoin())
        {
            Debug.LogWarning($"CoinSpawner: 平台 {platform.name} 已有金币，跳过");
            return;
        }
        
        var renderer = platform.GetComponent<Renderer>();
        if (renderer == null) return;
        
        var bounds = renderer.bounds;
        
        float randomX = Random.Range(bounds.min.x + edgePadding, bounds.max.x - edgePadding);
        float randomZ = Random.Range(bounds.min.z + edgePadding, bounds.max.z - edgePadding);
        float randomY = bounds.max.y + coinHeightOffset;
        
        Vector3 spawnPosition = new Vector3(randomX, randomY, randomZ);
        
        if (CoinPool.Instance != null)
        {
            GameObject coin = CoinPool.Instance.GetCoin(spawnPosition);
            
            if (coin != null)
            {
                platformComp.SetCoin(coin);
                
                Coin coinComp = coin.GetComponent<Coin>();
                if (coinComp != null)
                {
                    coinComp.SetParentPlatform(platformComp);
                }
            }
        }
    }
}