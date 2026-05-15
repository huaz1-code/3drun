using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Tooltip("跟随的目标")]
    public Transform target;
    
    [Tooltip("摄像机与目标的距离")]
    public float distance = 5f;
    
    [Tooltip("摄像机高度偏移")]
    public float height = 2f;
    
    [Tooltip("水平旋转速度")]
    public float rotationSpeedX = 2f;
    
    [Tooltip("垂直旋转速度")]
    public float rotationSpeedY = 1.5f;
    
    [Tooltip("最小俯视角度")]
    public float minVerticalAngle = -30f;
    
    [Tooltip("最大仰视角度")]
    public float maxVerticalAngle = 60f;
    
    [Tooltip("平滑跟随速度")]
    public float followSmoothSpeed = 5f;
    
    private float currentHorizontalAngle = 0f;
    private float currentVerticalAngle = 10f;
    
    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.Find("player");
            if (player != null)
                target = player.transform;
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        currentHorizontalAngle += mouseX * rotationSpeedX;
        currentVerticalAngle -= mouseY * rotationSpeedY;
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
        
        Quaternion rotation = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle, 0);
        
        Vector3 targetPosition = target.position + Vector3.up * height;
        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward) * distance;
        
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothSpeed * Time.deltaTime);
        transform.rotation = rotation;
    }
}
