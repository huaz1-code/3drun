using UnityEngine;
using System.Collections.Generic;

public class BossBulletPool : MonoBehaviour
{
    public static BossBulletPool Instance;

    [Tooltip("初始池子大小")]
    public int initialPoolSize = 20;

    [Tooltip("最大池子大小")]
    public int maxPoolSize = 50;

    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject bullet = new GameObject("BossBullet_" + i);
            bullet.AddComponent<BossBullet>();
            bullet.SetActive(false);
            bullet.transform.SetParent(transform);
            bulletPool.Enqueue(bullet);
        }
    }

    public GameObject GetBullet(Vector3 position, Vector3 direction)
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            BossBullet bulletComp = bullet.GetComponent<BossBullet>();
            bulletComp.Initialize(position, direction);
            return bullet;
        }
        else if (transform.childCount < maxPoolSize)
        {
            GameObject bullet = new GameObject("BossBullet_" + transform.childCount);
            BossBullet bulletComp = bullet.AddComponent<BossBullet>();
            bullet.transform.SetParent(transform);
            bulletComp.Initialize(position, direction);
            return bullet;
        }

        return null;
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }
}
