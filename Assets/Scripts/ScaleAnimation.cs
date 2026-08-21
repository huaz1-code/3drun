using UnityEngine;

[RequireComponent(typeof(Transform))]
public class ScaleAnimation : MonoBehaviour
{
    [Tooltip("缩放速度（每秒完成的周期数）")]
    public float speed = 1f;
    
    [Tooltip("缩放幅度（相对于原始大小的倍数）")]
    public float scaleAmount = 0.5f;
    
    [Tooltip("是否在启动时自动播放")]
    public bool playOnStart = true;
    
    private Vector3 originalScale;
    private float elapsedTime;
    private bool isAnimating;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void Start()
    {
        if (playOnStart)
        {
            StartAnimation();
        }
    }

    void Update()
    {
        if (isAnimating)
        {
            elapsedTime += Time.deltaTime;
            
            float scaleFactor = 1f + Mathf.Sin(elapsedTime * speed * Mathf.PI * 2f) * scaleAmount;
            
            transform.localScale = new Vector3(
                originalScale.x * scaleFactor,
                originalScale.y,
                originalScale.z * scaleFactor
            );
        }
    }

    [ContextMenu("Start Animation")]
    public void StartAnimation()
    {
        isAnimating = true;
        elapsedTime = 0f;
    }

    [ContextMenu("Stop Animation")]
    public void StopAnimation()
    {
        isAnimating = false;
        transform.localScale = originalScale;
    }

    [ContextMenu("Pause Animation")]
    public void PauseAnimation()
    {
        isAnimating = false;
    }

    [ContextMenu("Resume Animation")]
    public void ResumeAnimation()
    {
        isAnimating = true;
    }

    [ContextMenu("Reset Scale")]
    public void ResetScale()
    {
        StopAnimation();
        originalScale = transform.localScale;
    }
}
