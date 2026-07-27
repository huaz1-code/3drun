using UnityEngine;

public class DebuffZoneTrigger : MonoBehaviour
{
    private BossDebuffZone parentZone;

    public void Initialize(BossDebuffZone zone)
    {
        parentZone = zone;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController == null || !playerController.IsInvincible())
            {
                parentZone?.OnPlayerEnterZone(other);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            parentZone?.OnPlayerExitZone(other);
        }
    }
}
