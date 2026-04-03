using UnityEngine;

public class EnemyAnimationLink : MonoBehaviour
{
    private UnityEngine.AI.NavMeshAgent _agent;
    private Animator _animator;

    void Awake()
    {
        _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        float currentSpeed = _agent.velocity.magnitude;
        _animator.SetFloat("Speed", currentSpeed);
    }
}
