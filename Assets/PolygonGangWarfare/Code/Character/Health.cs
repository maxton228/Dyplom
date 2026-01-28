using UnityEngine;
using UnityEngine.SceneManagement;
public class Health : MonoBehaviour
{
    [Header("Ќалаштуванн€")]
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
        Debug.Log($"{gameObject.name} отримав пораненн€! «алишилось: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"<color=red>{gameObject.name} «ј√»Ќ”¬!</color>");

        if (isPlayer)
        {
            // якщо помер гравець - перезавантажуЇмо сцену (Game Over)
            Debug.Log("Game Over");
        }
        else
        {
            // якщо помер ворог
            // 1. ¬имикаЇмо мозок ≥ нав≥гац≥ю
            var ai = GetComponent<TacticalEnemy>();
            if (ai != null) ai.enabled = false;

            var nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (nav != null) nav.enabled = false;

            // 2. ¬микаЇмо ф≥зику (Ragdoll), €кщо Ї, або просто видал€Їмо
            // Destroy(gameObject, 2f); // ¬идалити через 2 сек

            // јбо просто падаЇмо на б≥к (простий вар≥ант смерт≥)
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
