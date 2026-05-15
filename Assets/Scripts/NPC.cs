using UnityEngine;

public class NPC : MonoBehaviour
{
    [Tooltip("移动速度")]
    public float moveSpeed = 2f;
    
    [Tooltip("移动范围（左右）")]
    public float moveRange = 3f;
    
    private Vector3 startPosition;
    private float direction = 1f;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        float newX = transform.position.x + direction * moveSpeed * Time.deltaTime;
        
        if (newX >= startPosition.x + moveRange || newX <= startPosition.x - moveRange)
        {
            direction *= -1;
        }
        
        transform.position = new Vector3(Mathf.Clamp(newX, startPosition.x - moveRange, startPosition.x + moveRange), 
                                        transform.position.y, 
                                        transform.position.z);
    }
    
    public void ResetNPC()
    {
        startPosition = transform.position;
        direction = Random.Range(0, 2) == 0 ? 1f : -1f;
    }
}