using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Налаштування")]
    public float maxHealth = 100f;
    public bool isPlayer = false;

    private float currentHealth;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} отримав поранення! Залишилось: {currentHealth}");

        if (!isPlayer)
        {
            var ai = GetComponent<TacticalEnemy>();
            if (ai != null)
            {
                ai.OnTookDamage();
            }
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"<color=red>{gameObject.name} ЗАГИНУВ!</color>");

        if (isPlayer)
        {
            Debug.Log("Game Over");
        }
        else
        {
            if (TryGetComponent<TacticalEnemy>(out var ai)) ai.enabled = false;
            if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var nav)) nav.enabled = false;
            if (TryGetComponent<EnemyAwareness>(out var awareness)) awareness.enabled = false;

            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                int randomDeath = UnityEngine.Random.Range(0, 2);
                animator.SetInteger("DeathIndex", randomDeath);
                animator.SetTrigger("Die");
            }

            StartCoroutine(CleanUpCorpseRoutine());
        }
    }

    private IEnumerator CleanUpCorpseRoutine()
    {
        yield return new WaitForSeconds(4f);

        if (TryGetComponent<EnemyAnimationLink>(out var animLink)) Destroy(animLink);

        if (TryGetComponent<TacticalEnemy>(out var ai)) Destroy(ai);
        if (TryGetComponent<EnemyAwareness>(out var awareness)) Destroy(awareness);
        if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var nav)) Destroy(nav);

        if (TryGetComponent<Rigidbody>(out var rb)) Destroy(rb);

        if (TryGetComponent<Collider>(out var col)) Destroy(col);

        if (TryGetComponent<Animator>(out var anim)) Destroy(anim);

        Hitbox[] hitboxes = GetComponentsInChildren<Hitbox>();
        foreach (var hb in hitboxes)
        {
            if (hb.TryGetComponent<Collider>(out var boneCol)) Destroy(boneCol);
            Destroy(hb);
        }

        Destroy(this);
    }

    public void InitHealth(float value)
    {
        maxHealth = value;
        currentHealth = value;
    }
}