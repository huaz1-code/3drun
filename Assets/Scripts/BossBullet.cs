using UnityEngine;

public class BossBullet : MonoBehaviour
{
    [Tooltip("子弹伤害")]
    public float damage = 10f;

    [Tooltip("子弹速度")]
    public float speed = 5f;

    [Tooltip("子弹生命周期（秒）")]
    public float lifetime = 3f;

    [Tooltip("子弹颜色")]
    public Color bulletColor = Color.red;

    [Tooltip("碰撞检测延迟（秒），防止生成时立即与Boss碰撞")]
    public float collisionDelay = 0.2f;

    private Vector3 direction;
    private float lifeTimer;
    private bool isActive = false;
    private bool canCollide = false;

    void Start()
    {
        CreateBulletVisual();
    }

    void Update()
    {
        if (!isActive) return;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            ReturnToPool();
            return;
        }

        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive || !canCollide) return;

        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();
            PlayerController playerController = other.GetComponent<PlayerController>();

            if (playerHealth != null && !playerHealth.IsDead())
            {
                if (playerController != null && playerController.IsInvincible())
                {
                    ReturnToPool();
                    return;
                }

                PotionEffects potionEffects = playerHealth.GetComponent<PotionEffects>();
                if (potionEffects != null && potionEffects.CheckAndConsumeShield())
                {
                    playerController?.EnterInvincibility();
                    ReturnToPool();
                    return;
                }

                playerHealth.TakeDamage(damage);
            }

            ReturnToPool();
        }
        else if (!other.CompareTag("Boss"))
        {
            ReturnToPool();
        }
    }

    private void CreateBulletVisual()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "BulletSphere";
        sphere.transform.parent = transform;
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = Vector3.one * 0.4f;

        MeshRenderer renderer = sphere.GetComponent<MeshRenderer>();
        Material material = new Material(Shader.Find("Standard"));
        material.color = bulletColor;
        material.SetColor("_EmissionColor", bulletColor);
        material.EnableKeyword("_EMISSION");
        renderer.material = material;

        Destroy(sphere.GetComponent<SphereCollider>());

        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.2f;
    }

    public void Initialize(Vector3 pos, Vector3 dir)
    {
        transform.position = pos;
        direction = dir.normalized;
        lifeTimer = 0f;
        isActive = true;
        canCollide = false;
        gameObject.SetActive(true);

        Invoke(nameof(EnableCollision), collisionDelay);
    }

    private void EnableCollision()
    {
        canCollide = true;
    }

    private void ReturnToPool()
    {
        isActive = false;
        gameObject.SetActive(false);
        BossBulletPool.Instance.ReturnBullet(gameObject);
    }
}
