using UnityEngine;

public class AttackState : IEnemyState
{
    private TacticalEnemy _enemy;
    private float _nextFireTime;
    private float _repathTimer;

    private float _lastTimeSeen;

    public AttackState(TacticalEnemy enemy)
    {
        _enemy = enemy;
    }

    public void Enter()
    {
        Debug.Log("ÐÅÆÈÌ Â²×ÍÎ¯ ÀÒÀÊÈ: ÀÊÒÈÂÎÂÀÍÎ");
        _lastTimeSeen = Time.time;
    }

    public void Update()
    {
        bool canSeeNow = _enemy.CanSeePlayer();
        float distanceToPlayer = Vector3.Distance(_enemy.transform.position, _enemy.player.position);

        if (canSeeNow)
        {
            _enemy.LastKnownTargetPos = _enemy.player.position;
            _lastTimeSeen = Time.time;

            if (distanceToPlayer > _enemy.maxShootingRange)
            {
                HuntingLogic();
            }
            else
            {
                CombatLogic();
            }
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
        var playerHealth = _enemy.player.GetComponent<Health>();
        if (playerHealth != null && playerHealth.isDead)
        {
            _enemy.SetAiming(false);
            _enemy.ChangeState(_enemy.PatrolState);
            return;
        }
        _enemy.Agent.isStopped = true;

        _enemy.SetAiming(true);

        Vector3 dir = (_enemy.LastKnownTargetPos - _enemy.transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            _enemy.transform.rotation = Quaternion.Slerp(_enemy.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
        }

        if (_enemy.isReloading) return;

        if (_enemy.currentAmmo <= 0)
        {
            _enemy.ReloadWeapon();
            return;
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

        _enemy.SetAiming(false);

        if (Time.time > _repathTimer)
        {
            _repathTimer = Time.time + 0.2f;
            _enemy.Agent.SetDestination(_enemy.LastKnownTargetPos);
        }
    }

    public void Exit()
    {
        _enemy.Agent.isStopped = false;
        _enemy.SetAiming(false);
    }
}