using UnityEngine;

public class SearchState : IEnemyState
{
    private TacticalEnemy _enemy;
    private float _searchTimer;
    private float _searchDuration = 7f;
    private bool _isMovingToRandomPoint = false;
    public SearchState(TacticalEnemy enemy) => _enemy = enemy;

    public void Enter()
    {
        Debug.Log("Починається пошук...");
        _enemy.Agent.isStopped = false;
        _enemy.Agent.speed = 2f;
        _enemy.Agent.SetDestination(_enemy.LastKnownTargetPos);
        _searchTimer = Time.time;
        _isMovingToRandomPoint = false;
    }

    public void Update()
    {
        if (_enemy.CanSeePlayer())
        {
            if (_enemy.Awareness.IsAlerted)
            {
                _enemy.ChangeState(_enemy.AttackState);
                return;
            }
            else
            {
                _enemy.ChangeState(_enemy.ChaseState);
                return;
            }
        }

        if (!_enemy.Agent.pathPending && _enemy.Agent.remainingDistance < 1f)
        {
            if (Time.time - _searchTimer > _searchDuration)
            {
                if (!_enemy.Awareness.IsAlerted)
                {
                    Debug.Log("Нікого не знайшов, повертаюсь до патруля.");
                    _enemy.ChangeState(_enemy.PatrolState);
                }
                else
                {
                    MoveToRandomPointNear(_enemy.LastKnownTargetPos);
                    _searchTimer = Time.time;
                }
            }
        }
    }

    private void MoveToRandomPointNear(Vector3 center)
    {
        Vector3 randomPos = center + Random.insideUnitSphere * 10f;
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
        {
            _enemy.Agent.SetDestination(hit.position);
        }
    }

    public void Exit() => Debug.Log("Пошук завершено.");
}