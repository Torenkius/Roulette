using UnityEngine;

public class Participant : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 4;
    public int currentHealth;
    public string participantName;
    public bool isDead = false;

    public virtual void Setup()
    {
        currentHealth = maxHealth;
        isDead = false;
        // UI güncellemesi eklenecek
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"{participantName} took {damage} damage. Current HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        currentHealth = 0;
        isDead = true;
        Debug.Log($"{participantName} Eliminated!");
    }
}
