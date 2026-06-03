using UnityEngine;
using System.Collections;

public class BossAttack : MonoBehaviour
{
    [Tooltip("Boss攻击伤害值")]
    public float attackDamage = 20f;

    [Tooltip("攻击冷却时间（秒）")]
    public float attackCooldown = 2f;

    private bool isOnCooldown = false;
    private Health playerHealth;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerHealth = collision.gameObject.GetComponent<Health>();
            TryAttack();
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TryAttack();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerHealth = other.gameObject.GetComponent<Health>();
            TryAttack();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (isOnCooldown) return;
        if (playerHealth == null || playerHealth.IsDead()) return;

        playerHealth.TakeDamage(attackDamage);
        StartCoroutine(AttackCooldownCoroutine());
    }

    private IEnumerator AttackCooldownCoroutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        isOnCooldown = false;
    }
}