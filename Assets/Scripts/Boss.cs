using UnityEngine;

public class Boss : MonoBehaviour
{
    [Tooltip("移动速度")]
    public float moveSpeed = 3f;

    [Tooltip("旋转速度")]
    public float rotationSpeed = 5f;

    [Tooltip("是否开始追踪玩家")]
    public bool isChasing = false;

    private Transform player;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // 冻结物理旋转，只允许代码控制旋转
        rb.freezeRotation = true;
        
        // 确保重力设置正确
        rb.useGravity = true;
        
        // 确保对象有正确的标签
        if (gameObject.tag != "Boss")
        {
            Debug.LogWarning("Boss对象没有设置Boss标签，请在Unity编辑器中设置标签为Boss！");
        }
        
        FindPlayer();
    }

    void Update()
    {
        if (isChasing && player != null)
        {
            UpdateRotation();
        }
    }

    void FixedUpdate()
    {
        if (isChasing && player != null)
        {
            ChasePlayer();
        }
    }

    public void StartChasing()
    {
        isChasing = true;
        FindPlayer();
        Debug.Log("Boss开始追踪玩家！");
    }

    public void StopChasing()
    {
        isChasing = false;
        Debug.Log("Boss停止追踪玩家");
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    private void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        Vector3 targetPosition = transform.position + direction * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    private void UpdateRotation()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (direction.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 碰撞时保持当前旋转，防止物理系统修改
        rb.angularVelocity = Vector3.zero;
    }

    private void OnCollisionStay(Collision collision)
    {
        // 持续碰撞时重置角速度
        rb.angularVelocity = Vector3.zero;
    }
}