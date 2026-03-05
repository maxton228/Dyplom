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
    [SerializeField] private float rotationSpeed = 450f; 

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

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    public void RegisterNoise(Vector3 noisePos, float volume)
    {
        lastNoisePosition = noisePos;
        hasHeardNoise = true;
        noiseTimer = noiseMemoryDuration;

    }

    public override void OnEpisodeBegin()
    {
        stepCount = 0;
        hasHeardNoise = false;
        noiseTimer = 0f;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.localPosition = new Vector3(0f, 0.5f, 0f);
        transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        if (target != null)
        {
            Vector3 randomDir = Random.onUnitSphere;
            randomDir.y = 0;
            randomDir.Normalize();

            float randomDistance = Random.Range(5f, 7f);

            target.localPosition = transform.localPosition + (randomDir * randomDistance);

            Vector3 finalPos = target.localPosition;
            finalPos.y = 0.5f;
            target.localPosition = finalPos;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.forward);

        sensor.AddObservation(rb.linearVelocity.x / moveSpeed);
        sensor.AddObservation(rb.linearVelocity.z / moveSpeed);

        sensor.AddObservation(hasHeardNoise ? 1.0f : 0.0f);

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

    public override void OnActionReceived(ActionBuffers actions)
    {
        stepCount++;

        if (hasHeardNoise)
        {
            noiseTimer -= Time.fixedDeltaTime;
            if (noiseTimer <= 0) hasHeardNoise = false;
        }

        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        Vector3 moveDir = new Vector3(moveX, 0f, moveZ).normalized;

        if (moveDir.magnitude > 0.1f)
        {
            Vector3 targetPos = rb.position + moveDir * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPos);

            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }

        AddReward(-0.0005f);

        if (hasHeardNoise)
        {
            float distToNoise = Vector3.Distance(transform.position, lastNoisePosition);
            if (distToNoise < 1.5f)
            {
                AddReward(0.1f);
                hasHeardNoise = false;
            }
        }

        if (stepCount > 2500)
        {
            AddReward(-1f);
            EndEpisode();
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Caught Player!");
            AddReward(10.0f); 
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Wall")) 
        {
            AddReward(-0.5f); 
        }
    }
}