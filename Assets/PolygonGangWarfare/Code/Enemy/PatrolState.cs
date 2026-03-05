using UnityEngine;

public class PatrolState : IEnemyState
{
    private TacticalEnemy _enemy;
    private int _patrolIndex = 0;

    public PatrolState(TacticalEnemy enemy)
    {
        _enemy = enemy;
    }

    public void Enter()
    {
        _enemy.Agent.isStopped = false;
        GoToNextPoint();
    }

    public void Update()
    {

        if (!_enemy.Agent.pathPending && _enemy.Agent.remainingDistance < 0.5f)
        {
            GoToNextPoint();
        }
    }

    public void Exit() { }

    private void GoToNextPoint()
    {
        if (_enemy.patrolPoints.Length == 0) return;
        _enemy.Agent.SetDestination(_enemy.patrolPoints[_patrolIndex].position);
        _patrolIndex = (_patrolIndex + 1) % _enemy.patrolPoints.Length;
    }
}
