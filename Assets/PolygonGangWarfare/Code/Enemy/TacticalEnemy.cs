using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class TacticalEnemy : MonoBehaviour
{
    [Header("Зір")]
    [Range(0, 360)] public float viewAngle = 110f;
    public Transform eyePoint;
    public LayerMask visionMask;

    [Header("Цілі")]
    public Transform player;
    public Transform[] patrolPoints;

    [Header("Налаштування Складності")]
    public EnemyStats stats; 

    [Header("Налаштування")]
    public Transform shootingPoint; 
    public float lostTime = 5f;

    public enum State { Patrol, Chase, Attack, Search }
    [Header("Діагностика")]
    public State currentState;

    private NavMeshAgent agent;
    private Rigidbody rb;
    private Vector3 lastKnownPos;
    private int patrolIndex = 0;
    private Coroutine searchRoutine;
    private float nextFireTime; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        var healthScript = GetComponent<Health>();
        if (healthScript != null && stats != null)
        {
            healthScript.InitHealth(stats.maxHealth);
        }

        rb.isKinematic = true;
        currentState = State.Patrol;
        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (stats == null)
        {
            Debug.LogError("НЕ ПРИЗНАЧЕНО EnemyStats (профіль складності)!");
            return;
        }

        bool canSeePlayer = CheckVision();

        if (canSeePlayer)
        {
            lastKnownPos = player.position;

            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= stats.visionRange * 0.7f)
                ChangeState(State.Attack);
            else
                ChangeState(State.Chase);
        }
        else
        {
            if (currentState == State.Chase || currentState == State.Attack)
            {
                ChangeState(State.Search);
            }
        }

        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic();
                break;
            case State.Chase:
                ChaseLogic();
                break;
            case State.Attack:
                AttackLogic();
                break;
            case State.Search:
                break;
        }
    }


    void PatrolLogic()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPatrolPoint();
        }
    }

    void ChaseLogic()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void AttackLogic()
    {
        agent.isStopped = true;

        if (player != null)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
        }

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + stats.fireRate;
        }

        Debug.DrawRay(transform.position + Vector3.up, transform.forward * stats.visionRange, Color.red);
    }

    void Shoot()
    {
        if (shootingPoint == null)
        {
            Debug.LogError("Не призначено Shooting Point у ворога!");
            return;
        }

        Vector3 direction = (player.position + Vector3.up * 1.5f) - shootingPoint.position;

        float xError = Random.Range(-stats.accuracyError, stats.accuracyError);
        float yError = Random.Range(-stats.accuracyError, stats.accuracyError);
        direction += new Vector3(xError, yError, 0);

        RaycastHit hit;
        if (Physics.Raycast(shootingPoint.position, direction, out hit, stats.visionRange))
        {
            if (hit.transform.CompareTag("Player"))
            {
                var targetHealth = hit.transform.GetComponent<Health>();
                if (targetHealth != null) targetHealth.TakeDamage(stats.damage);
            }

            Debug.DrawLine(shootingPoint.position, hit.point, Color.yellow, 0.1f);
        }
    }

    void StartSearch()
    {
        Debug.Log("Втратив гравця! Починаю пошук...");
        agent.isStopped = false;
        agent.SetDestination(lastKnownPos);

        if (searchRoutine != null) StopCoroutine(searchRoutine);
        searchRoutine = StartCoroutine(SearchTimer());
    }

    void EndSearch()
    {
        ChangeState(State.Patrol);
    }

    IEnumerator SearchTimer()
    {
        yield return new WaitForSeconds(lostTime);
        EndSearch();
    }

    void ChangeState(State newState)
    {
        if (currentState == newState) return;

        if (currentState == State.Search)
        {
            if (searchRoutine != null) StopCoroutine(searchRoutine);
        }

        currentState = newState;

        if (currentState == State.Search)
        {
            StartSearch();
        }
        else if (currentState == State.Patrol)
        {
            GoToNextPatrolPoint();
        }
    }

    public void HearNoise(Vector3 noisePosition, float noiseRadius)
    {
        if (currentState == State.Attack || currentState == State.Chase) return;

        float dist = Vector3.Distance(transform.position, noisePosition);

        if (dist > noiseRadius) return;

        Debug.Log($"Почув шум! (Радіус: {noiseRadius}, Дистанція: {dist})");

        lastKnownPos = noisePosition;
        ChangeState(State.Search);
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[patrolIndex].position);
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    bool CheckVision()
    {
        if (player == null || stats == null) return false;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer > stats.visionRange) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);

        if (angleToPlayer > viewAngle / 2) return false;

        Vector3 origin = eyePoint != null ? eyePoint.position : transform.position + Vector3.up * 1.6f;
        Vector3 target = player.position + Vector3.up * 1.5f;
        Vector3 rayDirection = (target - origin).normalized;

        RaycastHit hit;
        if (Physics.Raycast(origin, rayDirection, out hit, stats.visionRange, visionMask))
        {
            if (hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (stats == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stats.visionRange);

        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * stats.visionRange);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * stats.visionRange);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}