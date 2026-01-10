using UnityEngine;
using UnityEngine.InputSystem; // Обов'язково

public class PlayerMovementRB : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 0.001f;
    [SerializeField] private float changeDirectionInterval = 1.4f;
    [SerializeField] private float arenaSize = 4f; // Межі арени (щоб не втік)

    [Header("Noise Settings")]
    [SerializeField] private float noiseInterval = 0.5f; // Як часто шуміти
    [SerializeField] private float noiseRadius = 10f;

    private Vector3 moveDir;
    private float timer;
    private float noiseTimer;

    // Стан "гучності": 0 = тихо, 1 = кроки, 2 = біг
    private float currentVolume = 1.0f;

    void Start()
    {
        PickNewDirection();
    }

    void Update()
    {
        // 1. РУХ
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        // Тримаємо в межах арени (проста перевірка)
        // (Можна замінити на стіни з колайдерами, тоді це не треба)
        if (transform.localPosition.x > arenaSize || transform.localPosition.x < -arenaSize ||
            transform.localPosition.z > arenaSize || transform.localPosition.z < -arenaSize)
        {
            PickNewDirection();
            // Повертаємо трохи назад, щоб не застряг
            transform.position = Vector3.MoveTowards(transform.position, Vector3.zero, 1f);
        }

        // Таймер зміни напрямку
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            PickNewDirection();
        }

        // 2. ГЕНЕРАЦІЯ ШУМУ
        // Дублер сам шукає агента і каже йому "Я тут"
        noiseTimer -= Time.deltaTime;
        if (noiseTimer <= 0)
        {
            EmitNoise();
            noiseTimer = noiseInterval;
        }
    }

    void PickNewDirection()
    {
        // Випадковий напрямок X, Z
        float x = Random.Range(-1f, 1f);
        float z = Random.Range(-1f, 1f);
        moveDir = new Vector3(x, 0, z).normalized;

        // Випадково змінюємо "гучність" (імітуємо, що гравець то біжить, то крадеться)
        float rand = Random.value;
        if (rand < 0.3f) currentVolume = 0.0f; // Тиша (стоїть/крадеться)
        else if (rand < 0.7f) currentVolume = 1.0f; // Ходьба
        else currentVolume = 2.0f; // Біг

        timer = changeDirectionInterval;
    }

    void EmitNoise()
    {
        // Якщо ми зараз "тихі", то не шумимо
        if (currentVolume < 0.1f) return;

        // Шукаємо агентів поруч
        Collider[] hits = Physics.OverlapSphere(transform.position, noiseRadius);
        foreach (var hit in hits)
        {
            StealthAgent agent = hit.GetComponent<StealthAgent>();
            if (agent != null)
            {
                // Відправляємо сигнал, як це робив Movement.cs
                agent.RegisterNoise(transform.position, currentVolume);
            }
        }
    }
}