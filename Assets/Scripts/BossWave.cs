using UnityEngine;

public class BossWave : MonoBehaviour
{
    [Tooltip("圆环线条颜色")]
    public Color waveColor = Color.red;

    [Tooltip("圆环线条宽度")]
    public float lineWidth = 0.2f;

    [Tooltip("圆环分段数（越大越平滑）")]
    public int segments = 60;

    [Tooltip("攻击判定高度（玩家超过此高度可躲避）")]
    public float hitHeight = 1f;

    private float damage;
    private float maxRadius;
    private float speed;
    private float currentRadius = 0f;
    private bool isActive = false;
    private LineRenderer lineRenderer;
    private Transform playerTransform;
    private bool hasHitPlayer = false;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = waveColor;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;

        DrawCircle();
    }

    void Update()
    {
        if (!isActive) return;

        currentRadius += speed * Time.deltaTime;

        CheckRingCollision();
        DrawCircle();

        if (currentRadius >= maxRadius)
        {
            Destroy(gameObject);
        }
    }

    private void CheckRingCollision()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        if (playerTransform == null || hasHitPlayer) return;

        Vector3 playerPos = playerTransform.position;
        Vector3 wavePos = transform.position;

        float heightDiff = Mathf.Abs(playerPos.y - wavePos.y);
        if (heightDiff > hitHeight / 2f) return;

        Vector2 player2D = new Vector2(playerPos.x, playerPos.z);
        Vector2 wave2D = new Vector2(wavePos.x, wavePos.z);
        float distance = Vector2.Distance(player2D, wave2D);

        float innerRadius = currentRadius - lineWidth;
        float outerRadius = currentRadius + lineWidth;

        if (distance >= innerRadius && distance <= outerRadius)
        {
            Health playerHealth = playerTransform.GetComponent<Health>();
            if (playerHealth != null && !playerHealth.IsDead())
            {
                PotionEffects potionEffects = playerHealth.GetComponent<PotionEffects>();
                if (potionEffects == null || !potionEffects.CheckAndConsumeShield())
                {
                    playerHealth.TakeDamage(damage);
                    hasHitPlayer = true;
                }
            }
        }
    }

    private void DrawCircle()
    {
        Vector3[] points = new Vector3[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            points[i] = new Vector3(Mathf.Cos(angle) * currentRadius, 0.1f, Mathf.Sin(angle) * currentRadius);
        }
        lineRenderer.SetPositions(points);
    }

    public void Initialize(float damageValue, float radiusValue, float speedValue)
    {
        damage = damageValue;
        maxRadius = radiusValue;
        speed = speedValue;
        isActive = true;
    }
}