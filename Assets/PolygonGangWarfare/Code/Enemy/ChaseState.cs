using UnityEngine;

public class ChaseState : IEnemyState
{
    private TacticalEnemy _enemy;

    public ChaseState(TacticalEnemy enemy)
    {
        _enemy = enemy;
    }

    public void Enter()
    {
        _enemy.Agent.isStopped = false;
    }

    public void Update()
    {
        if (_enemy.CanSeePlayer())
        {
            _enemy.LastKnownTargetPos = _enemy.player.position;
            _enemy.Agent.SetDestination(_enemy.player.position);

        }
        else
        {
            _enemy.ChangeState(_enemy.SearchState);
        }
    }

    public void Exit() { }
}