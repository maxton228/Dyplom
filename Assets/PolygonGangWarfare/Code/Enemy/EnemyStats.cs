using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Game/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Здоров'я")]
    public float maxHealth = 100f;

    [Header("Бойові параметри")]
    public float damage = 10f;
    public float fireRate = 0.5f;

    [Tooltip("Відстань стрільби")]
    public float attackRange = 8f;

    [Tooltip("Розкид куль. Більше = кривіше")]
    public float accuracyError = 1.0f;
    

    [Header("Інтелект")]
    public float reactionTime = 1.0f;
    public float visionRange = 15f;
    public float hearingRange = 15f;
}
