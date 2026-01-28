using UnityEngine;

public class GameDifficultyManager : MonoBehaviour
{
    public enum Difficulty { Easy, Medium, Hard }
    public Difficulty currentDifficulty;

    [Header("Профілі")]
    public EnemyStats easyProfile;
    public EnemyStats mediumProfile;
    public EnemyStats hardProfile;

    void Start()
    {
        ApplyDifficulty();
    }

    public void SetDifficulty(int level)
    {
        currentDifficulty = (Difficulty)level;
        ApplyDifficulty();
    }

    void ApplyDifficulty()
    {
        EnemyStats profileToUse = mediumProfile;

        switch (currentDifficulty)
        {
            case Difficulty.Easy: profileToUse = easyProfile; break;
            case Difficulty.Medium: profileToUse = mediumProfile; break;
            case Difficulty.Hard: profileToUse = hardProfile; break;
        }

        TacticalEnemy[] enemies = FindObjectsOfType<TacticalEnemy>();

        foreach (var enemy in enemies)
        {
            enemy.stats = profileToUse;
            enemy.GetComponent<Health>().InitHealth(profileToUse.maxHealth);
        }

    }
}
