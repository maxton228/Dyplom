using UnityEngine;
using System.Collections.Generic;
public class StealthUIManager : MonoBehaviour
{
    [Header("Налаштування")]
    public GameObject indicatorPrefab; // Перевір, чи закинув сюди префаб в інспекторі!
    public Transform player;           // Перевір, чи закинув сюди гравця!

    private Dictionary<EnemyAwareness, StealthIndicator> _activeIndicators = new Dictionary<EnemyAwareness, StealthIndicator>();

    void Update()
    {
        // 1. Перевірка на null головних об'єктів
        if (indicatorPrefab == null || player == null) return;

        // 2. Очищуємо словник від видалених ворогів (щоб не було помилок)
        CleanupDestroyedEnemies();

        // 3. Проходимо по всіх ворогах
        foreach (var enemy in EnemyAwareness.AllEnemies)
        {
            // Перевірка, чи ворог взагалі існує в пам'яті
            if (enemy == null) continue;

            // Якщо рівень підозри високий і ворог ще не в стані бою
            if (enemy.currentAwareness > 0.05f && !enemy.IsAlerted)
            {
                if (!_activeIndicators.ContainsKey(enemy))
                {
                    CreateIndicator(enemy);
                }
            }
        }
    }

    void CreateIndicator(EnemyAwareness enemy)
    {
        GameObject go = Instantiate(indicatorPrefab, transform);
        StealthIndicator indicator = go.GetComponent<StealthIndicator>();

        if (indicator != null)
        {
            indicator.Setup(enemy, player);
            _activeIndicators.Add(enemy, indicator);
        }
        else
        {
            Debug.LogError("На префабі індикатора відсутній скрипт StealthIndicator!");
        }
    }

    void CleanupDestroyedEnemies()
    {
        List<EnemyAwareness> toRemove = new List<EnemyAwareness>();
        foreach (var pair in _activeIndicators)
        {
            if (pair.Key == null || pair.Value == null)
            {
                toRemove.Add(pair.Key);
            }
        }

        foreach (var key in toRemove)
        {
            _activeIndicators.Remove(key);
        }
    }
}