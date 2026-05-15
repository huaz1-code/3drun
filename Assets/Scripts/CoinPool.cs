using UnityEngine;
using System.Collections.Generic;

public class CoinPool : MonoBehaviour
{
    public static CoinPool Instance;
    
    public GameObject coinPrefab;
    public int initialPoolSize = 20;
    public int maxPoolSize = 50;
    
    private Queue<GameObject> coinPool = new Queue<GameObject>();
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        InitializePool();
    }
    
    void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject coin = Instantiate(coinPrefab);
            coin.SetActive(false);
            coin.transform.SetParent(transform);
            coinPool.Enqueue(coin);
        }
    }
    
    public GameObject GetCoin(Vector3 position)
    {
        if (coinPool.Count > 0)
        {
            GameObject coin = coinPool.Dequeue();
            coin.SetActive(true);
            coin.transform.position = position;
            
            Coin coinComp = coin.GetComponent<Coin>();
            if (coinComp != null)
            {
                coinComp.ResetCoin();
            }
            
            return coin;
        }
        else if (transform.childCount < maxPoolSize)
        {
            GameObject coin = Instantiate(coinPrefab);
            coin.transform.SetParent(transform);
            coin.transform.position = position;
            return coin;
        }
        
        return null;
    }
    
    public void ReturnCoin(GameObject coin)
    {
        coin.SetActive(false);
        coinPool.Enqueue(coin);
    }
}
