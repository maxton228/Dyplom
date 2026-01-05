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
    [SerializeField] private float moveSpeed = 4f;

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
    // EPISODE RESET
    // =========================
    public override void OnEpisodeBegin()
    {
        stepCount = 0;

        rb.linearVelocity = Vector3.zero; // (або rb.velocity для старих версій)
        rb.angularVelocity = Vector3.zero;

        // 1. Ставимо Агента (безпечно, щоб не в стіні)
        float safeZone = arenaSize - 2f;
        Vector3 agentPos = new Vector3(
            Random.Range(-safeZone, safeZone),
            0.5f,
            Random.Range(-safeZone, safeZone)
        );
        transform.localPosition = agentPos;
        // Можна навіть повернути його обличчям до цілі для початку, але поки хай крутиться
        transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        // 2. Ставимо Ціль ДУЖЕ БЛИЗЬКО (1.5 - 3 метри)
        float closeSpawnRadius = 3.0f; // Максимальна відстань
        float minSpawnDist = 1.5f;     // Мінімальна відстань

        Vector3 randomOffset;
        // Генеруємо випадкову точку, поки вона не потрапить у потрібний діапазон
        int attempts = 0;
        do
        {
            randomOffset = Random.insideUnitSphere * closeSpawnRadius;
            randomOffset.y = 0;
            attempts++;
        }
        while (randomOffset.magnitude < minSpawnDist && attempts < 10);

        // Якщо за 10 спроб не вийшло (малоймовірно), просто беремо вектор 2м вперед
        if (randomOffset.magnitude < minSpawnDist) randomOffset = Vector3.forward * 2f;

        Vector3 targetPos = agentPos + randomOffset;

        // 3. Clamp (щоб не вилізло за стіни)
        float limit = arenaSize - 1.5f;
        targetPos.x = Mathf.Clamp(targetPos.x, -limit, limit);
        targetPos.z = Mathf.Clamp(targetPos.z, -limit, limit);

        target.localPosition = targetPos;
    }

    // =========================
    // OBSERVATIONS
    // =========================
    public override void CollectObservations(VectorSensor sensor)
    {
       

        // Agent forward direction
        sensor.AddObservation(transform.forward);

        // Agent velocity
        sensor.AddObservation(rb.linearVelocity / moveSpeed);
    }

    // =========================
    // ACTIONS
    // =========================
    public override void OnActionReceived(ActionBuffers actions)
    {
        stepCount++;

        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        Vector3 moveDir = new Vector3(moveX, 0f, moveZ);

        // --- Physics movement ---
        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        // --- Rotation ---
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.fixedDeltaTime * 10f
            );
        }

        // =========================
        // REWARD LOGIC
        // =========================



        // Living penalty (forces speed)
        AddReward(-0.0005f);

        // Time limit
        if (stepCount > 2500)
        {
            AddReward(-1f);
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
            AddReward(50f);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.2f);
        }
    }

    // =========================
    // UTILS
    // =========================
    private Vector3 RandomPosition()
    {
        // Трішки зменшив радіус спавну агента, щоб він не з'являвся прямо в стіні
        float safeZone = arenaSize - 1.5f;
        return new Vector3(
            Random.Range(-safeZone, safeZone),
            0.5f,
            Random.Range(-safeZone, safeZone)
        );
    }

    // =========================
    // HEURISTIC (for testing)
    // =========================
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;

        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }
}
