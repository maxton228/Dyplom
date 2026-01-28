using UnityEngine;

public class NoiseMaker : MonoBehaviour
{
    [Header("Debug")]
    public bool showDebugVisuals = true;

    public void MakeSound(float radius)
    {
        if (radius <= 0) return;

        TacticalEnemy[] enemies = FindObjectsOfType<TacticalEnemy>();
        foreach (var enemy in enemies)
        {
            enemy.HearNoise(transform.position, radius);
        }
    }
}
