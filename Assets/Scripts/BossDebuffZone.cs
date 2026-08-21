using UnityEngine;
using System.Collections;

public class BossDebuffZone : MonoBehaviour
{
    [Tooltip("减速场半径")]
    public float zoneRadius = 5f;

    [Tooltip("减速场高度")]
    public float zoneHeight = 3f;

    [Tooltip("减速场颜色")]
    public Color zoneColor = new Color(1f, 1f, 0f, 0.3f);

    [Tooltip("减速场线条颜色")]
    public Color lineColor = Color.yellow;

    [Tooltip("减速场线条宽度")]
    public float lineWidth = 0.1f;

    [Tooltip("减速场分段数")]
    public int segments = 60;

    [Tooltip("减速比例（0-1，0表示完全停止，1表示不减速）")]
    public float slowFactor = 0.5f;

    [Tooltip("血量阈值百分比（低于此值时激活减速场）")]
    public float healthThreshold = 0.5f;

    [Tooltip("子弹发射间隔（秒）")]
    public float bulletInterval = 2f;

    [Tooltip("每次发射子弹数量")]
    public int bulletCount = 8;

    [Tooltip("子弹速度")]
    public float bulletSpeed = 5f;

    private Health health;
    private LineRenderer lineRenderer;
    private GameObject zoneVisual;
    public bool isActive = false;

    void Start()
    {
        health = GetComponent<Health>();
        if (health == null)
        {
            Debug.LogWarning("BossDebuffZone: 未找到 Health 组件！");
            return;
        }

        health.OnHealthChanged += OnHealthChanged;
        CheckHealthThreshold();
    }

    void OnHealthChanged(float current, float max, float change)
    {
        CheckHealthThreshold();
    }

    private void CheckHealthThreshold()
    {
        float healthPercent = health.GetHealthPercent();
        bool shouldActivate = healthPercent < healthThreshold && !health.IsDead();

        if (shouldActivate && !isActive)
        {
            ActivateZone();
        }
        else if (!shouldActivate && isActive)
        {
            DeactivateZone();
        }
    }

    private void ActivateZone()
    {
        isActive = true;
        Debug.Log("Boss减速场激活！");

        CreateVisual();
        StartCoroutine(BulletFireCoroutine());
    }

    private void DeactivateZone()
    {
        isActive = false;
        Debug.Log("Boss减速场关闭！");

        StopAllCoroutines();

        if (zoneVisual != null)
        {
            Destroy(zoneVisual);
            zoneVisual = null;
        }
    }

    private IEnumerator BulletFireCoroutine()
    {
        while (isActive && !health.IsDead())
        {
            FireBullets();
            yield return new WaitForSeconds(bulletInterval);
        }
    }

    private void FireBullets()
    {
        if (BossBulletPool.Instance == null)
        {
            CreateBulletPool();
        }

        Vector3 spawnPos = transform.position + new Vector3(0, 2f, 0);

        int firedCount = 0;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = (float)i / bulletCount * Mathf.PI * 2f;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

            GameObject bullet = BossBulletPool.Instance.GetBullet(spawnPos, direction);
            if (bullet != null)
            {
                BossBullet bulletComp = bullet.GetComponent<BossBullet>();
                bulletComp.speed = bulletSpeed;
                firedCount++;
            }
        }

        Debug.Log($"Boss发射子弹！数量: {firedCount}, 位置: {spawnPos}");
    }

    private void CreateBulletPool()
    {
        GameObject poolObj = new GameObject("BossBulletPool");
        BossBulletPool pool = poolObj.AddComponent<BossBulletPool>();
        BossBulletPool.Instance = pool;
        Debug.Log("自动创建BossBulletPool！");
    }

    private void CreateVisual()
    {
        zoneVisual = new GameObject("DebuffZoneVisual");
        zoneVisual.transform.parent = transform;
        zoneVisual.transform.localPosition = Vector3.zero;

        MeshRenderer renderer = zoneVisual.AddComponent<MeshRenderer>();
        MeshFilter filter = zoneVisual.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = new Vector3(0, 0, 0);
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * zoneRadius, 0, Mathf.Sin(angle) * zoneRadius);
        }
        vertices[segments + 1] = vertices[1];

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        filter.mesh = mesh;

        Material material = new Material(Shader.Find("Unlit/Transparent"));
        material.color = zoneColor;
        renderer.material = material;

        GameObject ring = new GameObject("ZoneRing");
        ring.transform.parent = zoneVisual.transform;
        ring.transform.localPosition = new Vector3(0, 0.05f, 0);

        lineRenderer = ring.AddComponent<LineRenderer>();
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = lineColor;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;

        Vector3[] ringPoints = new Vector3[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            ringPoints[i] = new Vector3(Mathf.Cos(angle) * zoneRadius, 0, Mathf.Sin(angle) * zoneRadius);
        }
        lineRenderer.SetPositions(ringPoints);

        GameObject colliderObj = new GameObject("ZoneCollider");
        colliderObj.transform.parent = zoneVisual.transform;
        colliderObj.transform.localPosition = Vector3.zero;

        SphereCollider sphereCollider = colliderObj.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = zoneRadius;

        DebuffZoneTrigger debuffTrigger = colliderObj.AddComponent<DebuffZoneTrigger>();
        debuffTrigger.Initialize(this);
    }

    public void OnPlayerEnterZone(Collider player)
    {
        if (!isActive) return;

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetSlowFactor(slowFactor);
            Debug.Log("玩家进入Boss减速场！");
        }
    }

    public void OnPlayerExitZone(Collider player)
    {
        if (!isActive) return;

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetSlowFactor(1f);
            Debug.Log("玩家离开Boss减速场！");
        }
    }

    

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnHealthChanged -= OnHealthChanged;
        }
    }
}
