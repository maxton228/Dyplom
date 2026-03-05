using UnityEngine;

public class NoiseMaker : MonoBehaviour
{
    [Header("Debug")]
    public bool showDebugVisuals = true;

    public void MakeSound(float radius, bool isGunshot)
    {
        TacticalEnemy[] enemies = FindObjectsByType<TacticalEnemy>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.HearNoise(transform.position, radius, isGunshot);
        }
    }
}
