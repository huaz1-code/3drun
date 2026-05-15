using UnityEngine;

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
    private Platform parentPlatform;
    
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
            CoinCounter.AddCoin();
            
            if (parentPlatform != null)
            {
                parentPlatform.SetCoin(null);
            }
            
            if (CoinPool.Instance != null)
            {
                CoinPool.Instance.ReturnCoin(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    
    public void ResetCoin()
    {
        originalPosition = transform.position;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
        parentPlatform = null;
    }
    
    public void SetParentPlatform(Platform platform)
    {
        parentPlatform = platform;
    }
}