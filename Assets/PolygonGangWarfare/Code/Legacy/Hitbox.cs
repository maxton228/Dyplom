using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Налаштування")]
    [Tooltip("Множник шкоди. Голова = 2.0 (або більше), Тіло = 1.0, Кінцівки = 0.5")]
    public float damageMultiplier = 1.0f;

    [Tooltip("Посилання на головне здоров'я ворога")]
    public Health mainHealth;

    public void ApplyDamage(float baseDamage)
    {
        if (mainHealth != null)
        {
            float finalDamage = baseDamage * damageMultiplier;
            Debug.Log($"Влучання в {gameObject.name}! Множник: {damageMultiplier}. Шкода: {finalDamage}");
            mainHealth.TakeDamage(finalDamage);
        }
    }
}