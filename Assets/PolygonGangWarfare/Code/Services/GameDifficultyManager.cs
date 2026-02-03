using UnityEngine;
using System;
public class DifficultyManager : MonoBehaviour, IDifficultyService
{
    public enum Difficulty { Easy, Medium, Hard }
    public Difficulty currentDifficulty;
    public event Action OnDifficultyChanged;

    [Header("Профілі")]
    public EnemyStats easyProfile;
    public EnemyStats mediumProfile;
    public EnemyStats hardProfile;

    public EnemyStats GetCurrentStats()
    {
        return currentDifficulty switch
        {
            Difficulty.Easy => easyProfile,
            Difficulty.Hard => hardProfile,
            _ => mediumProfile
        };
    }
    public void SetDifficulty(int level)
    {
        currentDifficulty = (Difficulty)level;
        ApplyChanges();
    }

    private void ApplyChanges()
    {
        Debug.Log($"[Difficulty] Складність змінено на: {currentDifficulty}");
        OnDifficultyChanged?.Invoke();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyChanges();
        }
    }
}
