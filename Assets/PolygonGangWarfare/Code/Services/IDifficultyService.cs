using UnityEngine;
using InfimaGames.LowPolyShooterPack;
using System;
public interface IDifficultyService : IGameService
{
    EnemyStats GetCurrentStats();
    void SetDifficulty(int level);
    event Action OnDifficultyChanged;
}