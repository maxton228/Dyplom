using UnityEngine;
using System.Collections;

public class DeadState : IEnemyState
{
    private TacticalEnemy _enemy;

    public DeadState(TacticalEnemy enemy) => _enemy = enemy;

    public void Enter()
    {
        if (_enemy.Agent.isActiveAndEnabled && _enemy.Agent.isOnNavMesh)
            _enemy.Agent.isStopped = true;

        _enemy.StartCoroutine(DeathProcess());
    }

    private IEnumerator DeathProcess()
    {
        yield return new WaitForSeconds(1.5f);

        Transform gunTransform = null;
        foreach (Transform child in _enemy.GetComponentsInChildren<Transform>())
        {
            if (child.name == "Gun")
            {
                gunTransform = child;
                break;
            }
        }


        if (gunTransform != null)
        {
            GameObject gun = gunTransform.gameObject;

            gun.transform.SetParent(null);

            if (gun.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true;
                rb.useGravity = true;
            }

            if (gun.TryGetComponent<Collider>(out var col))
                col.enabled = false;
        }
        else
        {
            Debug.LogWarning("Об'єкт Gun не знайдено");
        }

        Animator anim = _enemy.GetComponent<Animator>();
        if (anim != null) anim.enabled = false;

        _enemy.enabled = false;
    }

    public void Update() { }
    public void Exit() { }
}