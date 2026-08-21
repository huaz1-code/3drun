using UnityEngine;

/// <summary>
/// 金币脚本 - 控制单个金币的行为
/// 功能说明：
/// 1. 金币旋转动画
/// 2. 金币上下浮动效果
/// 3. 玩家碰撞检测
/// 4. 被收集后直接销毁
/// </summary>
public class Coin : MonoBehaviour
{
    [Tooltip("旋转速度")]
    public float rotationSpeed = 180f;

    [Tooltip("浮动高度")]
    public float floatHeight = 0.3f;

    [Tooltip("浮动速度")]
    public float floatSpeed = 2f;

    private Vector3 originalPosition;
    private float timeOffset;

    void Start()
    {
        originalPosition = transform.position;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        float offsetY = Mathf.Sin(Time.time * floatSpeed + timeOffset) * floatHeight;
        transform.position = originalPosition + new Vector3(0, offsetY, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CoinUIManager.AddCoin();
            Destroy(gameObject);
        }
    }
}
