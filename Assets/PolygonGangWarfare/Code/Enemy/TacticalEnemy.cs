using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic; 
using InfimaGames.LowPolyShooterPack;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemyAwareness))]
public class TacticalEnemy : MonoBehaviour
{
    private IDifficultyService difficultyService;

    [Header("Зір")]
    [Range(0, 360)] public float viewAngle = 110f;
    public Transform eyePoint;
    public LayerMask visionMask;

    [Header("Цілі")]
    public Transform player;
    public Transform[] patrolPoints;

    [Header("Налаштування Складності")]
    public EnemyStats Stats;

    [Header("Налаштування")]
    public Transform shootingPoint;
    public float lostTime = 5f;
    private float _visionTimer = 0f;
    public float reactionThreshold = 0.2f;

    [Header("Просунутий Зір (New)")]
    public LayerMask obstacleMask;

    public NavMeshAgent Agent { get; private set; }
    public EnemyAwareness Awareness { get; private set; }
    private Rigidbody rb;
    private VisibilityTarget _playerVisibility;

    private IEnemyState _currentState;

    public PatrolState PatrolState { get; private set; }
    public ChaseState ChaseState { get; private set; }
    public AttackState AttackState { get; private set; }
    public SearchState SearchState { get; private set; }

    public Vector3 LastKnownTargetPos { get; set; }

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        Awareness = GetComponent<EnemyAwareness>();

        rb.isKinematic = true;

        PatrolState = new PatrolState(this);
        ChaseState = new ChaseState(this);
        AttackState = new AttackState(this);
        SearchState = new SearchState(this);
    }

    void Start()
    {
        if (player != null)
            _playerVisibility = player.GetComponent<VisibilityTarget>();

        difficultyService = ServiceLocator.Current.Get<IDifficultyService>();

        if (difficultyService != null)
        {
            difficultyService.OnDifficultyChanged += UpdateStats;
            UpdateStats();
        }

        Awareness.OnAlerted += () => {
            ChangeState(AttackState);
            NotifyAllies();
        };

        ChangeState(PatrolState);
    }

    void Update()
    {
        if (Stats == null) return;
        _currentState?.Update();

        if (!Awareness.IsAlerted)
        {
            float visibility = CalculateVisibilityFactor();

            if (visibility > 0)
            {
                LastKnownTargetPos = player.position;

                float distToPlayer = Vector3.Distance(transform.position, player.position);
                Vector3 dirToPlayer = (player.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dirToPlayer);

                Awareness.ProcessVision(visibility, distToPlayer, Stats.visionRange, angle);
            }
            else
            {
                Awareness.ProcessVision(0, 0, 0, 0);
            }

            if (Awareness.currentAwareness >= 0.2f)
            {
                if (_currentState == PatrolState)
                {
                    ChangeState(ChaseState);
                }
            }
            else if (Awareness.currentAwareness < 0.05f && _currentState == ChaseState)
            {
                ChangeState(SearchState);
            }
        }
    }

    public void ChangeState(IEnemyState newState)
    {
        if (Awareness.IsAlerted && newState == PatrolState) return;

        if (_currentState == newState) return;

        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void HearNoise(Vector3 noisePosition, float noiseRadius, bool isLoudGunshot)
    {
        float dist = Vector3.Distance(transform.position, noisePosition);
        if (dist > noiseRadius) return;

        if (isLoudGunshot)
        {
            LastKnownTargetPos = noisePosition;
            Awareness.TriggerInstantAlert();

            if (_currentState != AttackState)
            {
                ChangeState(AttackState);
            }
            return;
        }

        if (Awareness.IsAlerted)
        {
            LastKnownTargetPos = noisePosition;
            if (_currentState == SearchState)
            {
                Agent.SetDestination(noisePosition);
            }
        }
        else
        {
            Awareness.AddSuspicion(0.3f);
            LastKnownTargetPos = noisePosition;

            if (_currentState != SearchState && _currentState != AttackState)
            {
                ChangeState(SearchState);
            }
            else if (_currentState == SearchState)
            {
                Agent.SetDestination(noisePosition);
            }
        }
    }

    float CalculateVisibilityFactor()
    {
        if (player == null || Stats == null || _playerVisibility == null) return 0f;

        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer > Stats.visionRange) return 0f;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        if (angleToPlayer > viewAngle / 2) return 0f;

        List<Vector3> points = _playerVisibility.GetActivePoints();
        int visibleCount = 0;

        Vector3 eyePos = eyePoint != null ? eyePoint.position : transform.position + Vector3.up * 1.6f;

        foreach (var point in points)
        {
            if (!Physics.Linecast(eyePos, point, obstacleMask))
            {
                visibleCount++;
            }
        }

        return (float)visibleCount / points.Count;
    }

    public void PerformShoot()
    {
        if (shootingPoint == null)
        {
            Debug.LogError("Не призначено Shooting Point у ворога!");
            return;
        }

        Vector3 direction = (player.position + Vector3.up * 1.5f) - shootingPoint.position;

        float xError = Random.Range(-Stats.accuracyError, Stats.accuracyError);
        float yError = Random.Range(-Stats.accuracyError, Stats.accuracyError);
        direction += new Vector3(xError, yError, 0);

        RaycastHit hit;
        if (Physics.Raycast(shootingPoint.position, direction, out hit, Stats.visionRange))
        {
            if (hit.transform.CompareTag("Player"))
            {
                var targetHealth = hit.transform.GetComponent<Health>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(Stats.damage);
                }
            }

            Debug.DrawLine(shootingPoint.position, hit.point, Color.yellow, 0.1f);
        }
    }

    void UpdateStats()
    {
        if (difficultyService == null) return;

        Stats = difficultyService.GetCurrentStats();

        var healthScript = GetComponent<Health>();
        if (healthScript != null && Stats != null) 
        {
            healthScript.InitHealth(Stats.maxHealth); 
        }
    }

    void OnDestroy()
    {
        if (difficultyService != null)
        {
            difficultyService.OnDifficultyChanged -= UpdateStats;
        }
    }

    public bool CanSeePlayer()
    {
        return CalculateVisibilityFactor() > 0;
    }

    void OnDrawGizmosSelected()
    {
        if (Stats == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Stats.visionRange);

        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * Stats.visionRange); 
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * Stats.visionRange); 
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public void AlertFromAlly(Vector3 targetPos)
    {
        LastKnownTargetPos = targetPos;

        if (!Awareness.IsAlerted)
        {
            Awareness.TriggerInstantAlert();
        }

        if (_currentState != AttackState)
        {
            ChangeState(AttackState);
        }
    }
    private void NotifyAllies()
    {
        var allEnemies = EnemyAwareness.AllEnemies;
        foreach (var enemyAwareness in allEnemies)
        {
            if (enemyAwareness == null) continue;

            var ally = enemyAwareness.GetComponent<TacticalEnemy>();

            if (ally != null && ally != this && !ally.Awareness.IsAlerted)
            {
                ally.AlertFromAlly(player.position);
            }
        }
    }
}