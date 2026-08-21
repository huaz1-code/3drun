using UnityEngine;

public class DebuffZoneTrigger : MonoBehaviour
{
    private BossDebuffZone parentZone;
    private bool isPlayerSlowed = false;

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
                ApplySlow(other);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null && !playerController.IsInvincible() && !isPlayerSlowed)
            {
                ApplySlow(other);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RemoveSlow(other);
        }
    }

    private void ApplySlow(Collider player)
    {
        isPlayerSlowed = true;
        parentZone?.OnPlayerEnterZone(player);
    }

    private void RemoveSlow(Collider player)
    {
        isPlayerSlowed = false;
        parentZone?.OnPlayerExitZone(player);
    }
}
