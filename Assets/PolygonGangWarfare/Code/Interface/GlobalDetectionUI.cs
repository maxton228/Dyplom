using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
public class GlobalDetectionUI : MonoBehaviour
{
    public GameObject indicatorPrefab; // Префаб дуги/риски
    public Transform playerTransform;

    // Словник, щоб не створювати нові об'єкти кожен кадр
    private Dictionary<EnemyAwareness, GameObject> _indicators = new Dictionary<EnemyAwareness, GameObject>();
    public static List<EnemyAwareness> AllEnemies => EnemyAwareness.AllEnemies;
    void Update()
    {
        foreach (var enemy in AllEnemies)
        {
            if (enemy == null) continue;

            // Якщо ворог почав щось підозрювати
            if (enemy.currentAwareness > 0.05f)
            {
                if (!_indicators.ContainsKey(enemy))
                {
                    GameObject icon = Instantiate(indicatorPrefab, transform);
                    _indicators.Add(enemy, icon);
                }

                UpdateIndicator(enemy, _indicators[enemy]);
            }
            else if (_indicators.ContainsKey(enemy))
            {
                // Видаляємо, якщо ворог більше не бачить
                Destroy(_indicators[enemy]);
                _indicators.Remove(enemy);
            }
        }
    }

    void UpdateIndicator(EnemyAwareness enemy, GameObject indicator)
    {
        // 1. Рахуємо напрямок на ворога відносно гравця
        Vector3 directionToEnemy = enemy.transform.position - playerTransform.position;

        // 2. Проектуємо на площину (ігноруємо висоту Y)
        directionToEnemy.y = 0;
        Vector3 forward = playerTransform.forward;
        forward.y = 0;

        // 3. Рахуємо кут між "вперед" гравця та ворогом
        float angle = Vector3.SignedAngle(forward, directionToEnemy, Vector3.up);

        // 4. Обертаємо UI елемент (Z в UI — це обертання по колу)
        indicator.transform.localRotation = Quaternion.Euler(0, 0, -angle);

        // 5. Міняємо колір залежно від ступеня виявлення
        Image img = indicator.GetComponent<Image>();
        img.color = Color.Lerp(Color.white, Color.red, enemy.currentAwareness);

        // Можна також міняти прозорість, щоб вони плавно з'являлися
        var tempCol = img.color;
        tempCol.a = enemy.currentAwareness;
        img.color = tempCol;
    }
}
