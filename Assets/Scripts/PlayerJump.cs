using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [Tooltip("跳跃初始速度")]
    public float jumpVelocity = 8f;
    
    [Tooltip("重力加速度")]
    public float gravity = 20f;
    
    [Tooltip("地面检测盒子半宽")]
    public Vector3 groundCheckHalfExtents = new Vector3(0.45f, 0.05f, 0.45f);
    
    [Tooltip("地面检测盒子中心点偏移")]
    public Vector3 groundCheckOffset = new Vector3(0, -0.5f, 0);
    
    [Tooltip("地面层")]
    public LayerMask groundLayer;
    
    [Tooltip("是否打印调试信息")]
    public bool debugMode = true;
    
    private Rigidbody rb;
    private bool isGrounded = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            Debug.LogError("PlayerJump: 没有找到Rigidbody组件！");
            enabled = false;
            return;
        }
        
        rb.useGravity = false;
        
        if (groundLayer.value == 0)
        {
            Debug.LogWarning("PlayerJump: 请在Inspector中设置Ground Layer！");
        }
    }
    
    void Update()
    {
        HandleJumpInput();
    }
    
    void FixedUpdate()
    {
        isGrounded = IsOnFloor();
        ApplyGravity();
        
        if (debugMode)
        {
            Debug.Log($"地面检测: {isGrounded}, 速度Y: {rb.velocity.y:F2}");
        }
    }
    
    bool IsOnFloor()
    {
        if (groundLayer.value == 0)
            return false;
            
        Vector3 center = transform.position + groundCheckOffset;
        Collider[] hitColliders = Physics.OverlapBox(center, groundCheckHalfExtents, transform.rotation, groundLayer);
        
        return hitColliders.Length > 0;
    }
    
    void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
                if (debugMode)
                    Debug.Log("跳跃！");
            }
            else if (debugMode)
            {
                Debug.Log("不在地面上，无法跳跃");
            }
        }
    }
    
    void ApplyGravity()
    {
        if (!isGrounded)
        {
            rb.velocity += Vector3.down * gravity * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y < 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 center = transform.position + groundCheckOffset;
        Gizmos.DrawWireCube(center, groundCheckHalfExtents * 2);
    }
}