using UnityEngine;
using System.Collections.Generic;
public class VisibilityTarget : MonoBehaviour
{
    [Header("Налаштування сітки")]
    [Range(2, 10)] public int heightSegments = 5;
    [Range(2, 5)] public int widthSegments = 3;

    private CapsuleCollider _col;
    // Ми зберігаємо не точки, а "коефіцієнти" (від -0.5 до 0.5)
    private List<Vector2> _normalizedPositions = new List<Vector2>();

    // Кешуємо список для повернення, щоб не створювати сміття (Garbage Collection) кожен кадр
    private List<Vector3> _cachedWorldPoints = new List<Vector3>();

    void Awake()
    {
        _col = GetComponent<CapsuleCollider>();
        CalculateNormalizedGrid();
    }

    void CalculateNormalizedGrid()
    {
        _normalizedPositions.Clear();

        // Генеруємо сітку від -0.5 (низ/ліво) до 0.5 (верх/право)
        for (int y = 0; y < heightSegments; y++)
        {
            // tY йде від 0 до 1
            float tY = (float)y / (heightSegments - 1);
            // переводимо в діапазон -0.5 ... 0.5
            float yRatio = Mathf.Lerp(-0.5f, 0.5f, tY);

            for (int x = 0; x < widthSegments; x++)
            {
                float tX = (float)x / (widthSegments - 1);
                float xRatio = Mathf.Lerp(-0.5f, 0.5f, tX);

                _normalizedPositions.Add(new Vector2(xRatio, yRatio));
            }
        }
    }

    /// <summary>
    /// Цей метод викликає ворог. Він враховує ПОТОЧНУ висоту та радіус, навіть якщо гравець присів.
    /// </summary>
    public List<Vector3> GetActivePoints()
    {
        _cachedWorldPoints.Clear();

        // Отримуємо актуальні дані коллайдера
        float currentHeight = _col.height;
        float currentRadius = _col.radius;
        Vector3 centerOffset = _col.center; // Центр може зміщуватись при присіданні

        // Трохи звужуємо сітку (на 10-15%), щоб точки не були прямо на шкірі 
        // (це допомагає уникнути глюків, коли точка "в текстурі" стіни)
        float heightScale = currentHeight * 0.9f;
        float widthScale = currentRadius * 2f * 0.8f;

        foreach (var normPos in _normalizedPositions)
        {
            // 1. Розраховуємо локальну позицію відносно центру коллайдера
            // normPos.y - це відсоток висоти (-0.5 до 0.5)
            // normPos.x - це відсоток ширини

            Vector3 localPos = new Vector3(
                normPos.x * widthScale,   // X - ширина
                normPos.y * heightScale,  // Y - висота
                0                         // Z - по центру (або можна додати глибину, якщо треба)
            );

            // 2. Додаємо зміщення центру коллайдера (бо центр капсули не завжди в 0,0,0)
            localPos += centerOffset;

            // 3. Переводимо в світові координати, враховуючи поворот і позицію гравця
            Vector3 worldPos = transform.TransformPoint(localPos);

            _cachedWorldPoints.Add(worldPos);
        }

        return _cachedWorldPoints;
    }

    // Малювання для дебагу
    void OnDrawGizmos()
    {
        if (_col == null) _col = GetComponent<CapsuleCollider>();
        if (_col == null) return;

        // Щоб бачити точки в редакторі, емулюємо виклик
        if (!Application.isPlaying)
        {
            CalculateNormalizedGrid(); // Тільки для редактора
        }

        Gizmos.color = Color.yellow;
        var points = GetActivePoints(); // Використовуємо ту саму логіку
        foreach (var p in points)
        {
            Gizmos.DrawSphere(p, 0.05f);
        }
    }
}
