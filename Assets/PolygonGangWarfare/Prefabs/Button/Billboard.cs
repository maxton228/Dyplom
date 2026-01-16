using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Знаходимо головну камеру сцени.
        // Переконайся, що твоя камера гравця має тег "MainCamera"!
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            // Якщо раптом камери немає або тег не той, шукаємо будь-яку камеру
            mainCamera = FindFirstObjectByType<Camera>();
        }
    }

    // Використовуємо LateUpdate. Це важливо!
    // Це означає, що UI повернеться ПІСЛЯ того, як гравець і його камера вже посунулись у цьому кадрі.
    // Це запобігає дрижанню картинки.
    void LateUpdate()
    {
        if (mainCamera == null) return;

        // --- Магія Білбордингу для UI ---
        // Ми змушуємо цей об'єкт дивитися в точку, яка знаходиться прямо перед ним,
        // але орієнтуючись на поворот камери.
        // Це стандартний спосіб для 2D спрайтів у 3D світі, щоб вони не були перевернуті.
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                         mainCamera.transform.rotation * Vector3.up);

        // АЛЬТЕРНАТИВНИЙ ВАРІАНТ (ПРОСТІШИЙ):
        // Якщо верхній варіант чомусь працює дивно (наприклад, картинка лежить на боці),
        // закоментуй верхній рядок і розкоментуй нижній. Він просто копіює поворот камери.
        // transform.rotation = mainCamera.transform.rotation;
    }
}
