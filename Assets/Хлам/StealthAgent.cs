using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(Rigidbody))]
public class StealthAgent : Agent
{
    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [Tooltip("Швидкість повороту (градусів за секунду). Було 720, стало 200.")]
    [SerializeField] private float rotationSpeed = 450f; // <--- ЗМЕНШИВ ШВИДКІСТЬ

    [Header("Hearing System")]
    [Tooltip("Скільки секунд бот пам'ятає звук.")]
    [SerializeField] private float noiseMemoryDuration = 3.0f;
    private bool hasHeardNoise = false;
    private Vector3 lastNoisePosition;
    private float noiseTimer;

    [Header("Arena")]
    [SerializeField] private float arenaSize = 4f;
    [SerializeField] private float spawnRadius = 6f;

    
    private Rigidbody rb;
    private int stepCount;

    // =========================
    // INITIALIZATION
    // =========================
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // =========================
    // HEARING METHOD (Для Movement.cs)
    // =========================
    public void RegisterNoise(Vector3 noisePos, float volume)
    {
        // Бот запам'ятовує точку звуку
        lastNoisePosition = noisePos;
        hasHeardNoise = true;
        noiseTimer = noiseMemoryDuration; // Скидаємо таймер забування

        // Можна додати миттєвий маленький ревард, щоб він зрозумів, що слухати - це корисно
        // AddReward(0.001f); 
    }

    // =========================
    // EPISODE RESET
    // =========================
    public override void OnEpisodeBegin()
    {
        stepCount = 0;
        hasHeardNoise = false;
        noiseTimer = 0f;

        // Скидаємо фізику
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 1. БОТ: Стає чітко в центр своєї арени (локальні координати 0,0,0)
        transform.localPosition = new Vector3(0f, 0.5f, 0f);
        transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        // 2. ГРАВЕЦЬ (Target): Спавниться навколо бота
        if (target != null)
        {
            // Вибираємо випадковий напрямок
            Vector3 randomDir = Random.onUnitSphere;
            randomDir.y = 0; // Щоб не спавнився в повітрі або під підлогою
            randomDir.Normalize();

            // Вибираємо випадкову відстань (від 2 до 5 метрів)
            float randomDistance = Random.Range(5f, 7f);

            // Ставимо гравця: Позиція Бота + Напрямок * Відстань
            target.localPosition = transform.localPosition + (randomDir * randomDistance);

            // Корекція висоти гравця (щоб не провалився)
            Vector3 finalPos = target.localPosition;
            finalPos.y = 0.5f;
            target.localPosition = finalPos;
        }
    }

    // =========================
    // OBSERVATIONS (Вхідні дані нейронки)
    // =========================
    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Власний напрямок (Forward) - 3 числа
        sensor.AddObservation(transform.forward);

        // 2. Власна швидкість - 2 числа
        sensor.AddObservation(rb.linearVelocity.x / moveSpeed);
        sensor.AddObservation(rb.linearVelocity.z / moveSpeed);

        // 3. --- СЛУХ (Нове) --- 
        // 1 число: Чи чую я звук? (1 = так, 0 = ні)
        sensor.AddObservation(hasHeardNoise ? 1.0f : 0.0f);

        // 3 числа: Вектор напрямку на звук (якщо звуку немає, передаємо нуль)
        if (hasHeardNoise)
        {
            Vector3 dirToNoise = (lastNoisePosition - transform.position).normalized;
            sensor.AddObservation(dirToNoise);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
        }
    }

    // =========================
    // ACTIONS
    // =========================
    public override void OnActionReceived(ActionBuffers actions)
    {
        stepCount++;

        // --- 1. ЛОГІКА СЛУХУ ---
        if (hasHeardNoise)
        {
            noiseTimer -= Time.fixedDeltaTime;
            if (noiseTimer <= 0) hasHeardNoise = false;
        }

        // --- 2. РУХ (Виправлено дублювання) ---
        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        Vector3 moveDir = new Vector3(moveX, 0f, moveZ).normalized; // Додав normalized, щоб діагональний рух не був швидшим

        // Рухаємось
        if (moveDir.magnitude > 0.1f)
        {
            // Рух
            Vector3 targetPos = rb.position + moveDir * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPos);

            // Поворот (тільки якщо рухаємось)
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }

        // --- 3. НАГОРОДИ ---

        // Штраф за час (щоб поспішав)
        AddReward(-0.0005f);

        // Нагорода за те, що йде на звук (опціонально, щоб навчити користуватися слухом)
        if (hasHeardNoise)
        {
            float distToNoise = Vector3.Distance(transform.position, lastNoisePosition);
            // Якщо підійшов до джерела звуку - даємо трохи балів і "забуваємо" звук, щоб не тупив там
            if (distToNoise < 1.5f)
            {
                AddReward(0.1f);
                hasHeardNoise = false;
            }
        }

        // Завершення епізоду, якщо занадто довго тупить
        if (stepCount > 2500)
        {
            AddReward(-1f); // Штраф за поразку часом
            EndEpisode();
        }
    }

    // =========================
    // COLLISIONS
    // =========================
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Caught Player!");
            AddReward(10.0f); // Велика нагорода за піймання
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Wall")) // Краще перевіряти тег "Wall", а не "все що не підлога"
        {
            AddReward(-0.5f); // Маленький штраф за тикання в стіни
        }
    }
}