using UnityEngine;
using UnityEngine.InputSystem; // Обов'язково

public class PlayerMovementRB : MonoBehaviour
{
    [Header("Speeds")]
    [SerializeField] private float walkSpeed = 0.8f;
    [SerializeField] private float runSpeed = 1f;
    [SerializeField] private float sprintSpeed = 1.4f;

    [Header("Behavior Settings")]
    [SerializeField] private float changeDirectionInterval = 2f;
    [SerializeField] private float arenaSize = 4f;

    [Header("Noise Settings")]
    [SerializeField] private float noiseInterval = 0.5f;
    [SerializeField] private float baseNoiseRadius = 1f;

    private Vector3 moveDir;
    private float timer;
    private float noiseTimer;
    private Rigidbody rb;
    // Стан "гучності": 0 = тихо, 1 = кроки, 2 = біг
    private float currentSpeed;
    private float currentVolume;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        PickNewBehavior();
    }
    void FixedUpdate()
    {
        // Фізичний рух
        Vector3 newPos = rb.position + moveDir * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        // Поворот
        if (moveDir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(moveDir);
            rb.rotation = Quaternion.Slerp(rb.rotation, lookRot, Time.fixedDeltaTime * 10f);
        }
    }
    void Update()
    {
        if (transform.localPosition.magnitude > arenaSize)
        {
            moveDir = -transform.localPosition.normalized; // Повертаємось в центр
            // Коли повертаємось, скидаємо швидкість на звичайну, щоб не вилітав кулею
            currentSpeed = walkSpeed;
            currentVolume = 1.0f;
        }

        // Таймер зміни поведінки
        timer -= Time.deltaTime;
        if (timer <= 0) PickNewBehavior();

        // Таймер шуму
        noiseTimer -= Time.deltaTime;
        if (noiseTimer <= 0)
        {
            EmitNoise();
            noiseTimer = noiseInterval;
        }
    }

    void PickNewBehavior()
    {
        // 1. Вибираємо напрямок
        float x = Random.Range(-1f, 1f);
        float z = Random.Range(-1f, 1f);
        moveDir = new Vector3(x, 0, z).normalized;

        // 2. Вибираємо ШВИДКІСТЬ і ГУЧНІСТЬ (Лотерея)
        float rand = Random.value; // Число від 0.0 до 1.0

        if (rand < 0.1f)
        {
            // 10% ШАНС: СПРИНТ (Гучно - 4)
            currentSpeed = sprintSpeed;
            currentVolume = 4.0f;
            timer = 1.0f; // Спринт короткий
        }
        else if (rand < 0.3f)
        {
            // 20% ШАНС: БІГ (Гучно - 2)
            currentSpeed = runSpeed;
            currentVolume = 2.0f;
            timer = changeDirectionInterval;
        }
        else
        {
            // 70% ШАНС: ХОДЬБА (Тихо - 1)
            currentSpeed = walkSpeed;
            currentVolume = 1.0f;
            timer = changeDirectionInterval;
        }
    }
    // -------------------------------------

    void EmitNoise()
    {
        if (currentVolume < 0.1f) return;

        float effectiveRadius = baseNoiseRadius * currentVolume;

        Collider[] hits = Physics.OverlapSphere(transform.position, effectiveRadius);
        foreach (var hit in hits)
        {
            StealthAgent agent = hit.GetComponent<StealthAgent>();
            if (agent != null)
            {
                agent.RegisterNoise(transform.position, currentVolume);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, baseNoiseRadius * currentVolume);
    }
}