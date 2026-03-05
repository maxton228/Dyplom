using UnityEngine;
using UnityEngine.SceneManagement;
public class Health : MonoBehaviour
{
    [Header("Налаштування")]
    public float maxHealth = 100f;
    public bool isPlayer = false;

    private float currentHealth;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} отримав поранення! Залишилось: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"<color=red>{gameObject.name} ЗАГИНУВ!</color>");

        if (isPlayer)
        {
            Debug.Log("Game Over");
        }
        else
        {
            var ai = GetComponent<TacticalEnemy>();
            if (ai != null) ai.enabled = false;

            var nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (nav != null) nav.enabled = false;

            transform.Rotate(-90, 0, 0);
            Destroy(gameObject, 5f);
        }
    }
    public void InitHealth(float value)
    {
        maxHealth = value;
        currentHealth = value;
    }
}
