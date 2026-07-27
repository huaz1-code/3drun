using UnityEngine;

public class BossAttack : MonoBehaviour
{
    [Tooltip("Boss攻击伤害值")]
    public float attackDamage = 20f;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            ApplyDamage(playerHealth, playerController);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            ApplyDamage(playerHealth, playerController);
        }
    }

    private void ApplyDamage(Health playerHealth, PlayerController playerController)
    {
        if (playerHealth == null || playerHealth.IsDead()) return;
        if (playerController != null && playerController.IsInvincible()) return;

        PotionEffects potionEffects = playerHealth.GetComponent<PotionEffects>();
        if (potionEffects != null && potionEffects.CheckAndConsumeShield())
        {
            playerController?.EnterInvincibility();
            return;
        }

        playerHealth.TakeDamage(attackDamage);
    }
}