using UnityEngine;

public class AttackState : IEnemyState
{
    private TacticalEnemy _enemy;
    private float _nextFireTime;
    private float _repathTimer;

    private float _lastTimeSeen;
    private float _reactionDelay = 1.0f;

    public AttackState(TacticalEnemy enemy)
    {
        _enemy = enemy;
    }

    public void Enter()
    {
        Debug.Log("ÐÅÆÈÌ Â²×ÍÎ¯ ÀÒÀÊÈ: ÀÊÒÈÂÎÂÀÍÎ");
        _enemy.Agent.isStopped = false;
        _enemy.Agent.speed = 3.5f;
        _lastTimeSeen = Time.time;
    }

    public void Update()
    {
        bool canSeeNow = _enemy.CanSeePlayer();

        if (canSeeNow)
        {
            _enemy.LastKnownTargetPos = _enemy.player.position;
            _lastTimeSeen = Time.time;
            CombatLogic();
        }
        else
        {
            if (Time.time - _lastTimeSeen > _enemy.lostTime)
            {
                _enemy.ChangeState(_enemy.SearchState);
            }
            else
            {
                HuntingLogic();
            }
        }
    }

    void CombatLogic()
    {
        _enemy.Agent.isStopped = true;

        Vector3 dir = (_enemy.LastKnownTargetPos - _enemy.transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            _enemy.transform.rotation = Quaternion.Slerp(_enemy.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
        }

        if (Time.time >= _nextFireTime)
        {
            _enemy.PerformShoot();
            _nextFireTime = Time.time + _enemy.Stats.fireRate;
        }
    }

    void HuntingLogic()
    {
        _enemy.Agent.isStopped = false;

        if (Time.time > _repathTimer)
        {
            _repathTimer = Time.time + 0.2f;
            _enemy.Agent.SetDestination(_enemy.LastKnownTargetPos);
        }

        if (!_enemy.Agent.pathPending && _enemy.Agent.remainingDistance < 1f)
        {
        }
    }

    public void Exit()
    {
        _enemy.Agent.isStopped = false;
    }
}