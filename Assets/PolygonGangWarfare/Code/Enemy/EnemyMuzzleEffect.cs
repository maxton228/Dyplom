using UnityEngine;
using System.Collections;

public class EnemyMuzzleEffect : MonoBehaviour
{
    [Header("Ефекти")]
    public ParticleSystem muzzleFlash;
    public int flashParticlesCount = 5;

    [Header("Звук")]
    public AudioSource audioSource;
    public AudioClip shootSound;

    [Header("Налаштування Рандомної Черги (Burst)")]
    public int minBurstCount = 1;
    public int maxBurstCount = 5;
    public float burstInterval = 0.1f;

    [HideInInspector]
    public int burstCount;

    public void PlayEffect()
    {
        StopAllCoroutines();
        StartCoroutine(BurstRoutine());
    }

    private IEnumerator BurstRoutine()
    {
        if (maxBurstCount < minBurstCount) maxBurstCount = minBurstCount;

        burstCount = Random.Range(minBurstCount, maxBurstCount + 1);

        for (int i = 0; i < burstCount; i++)
        {
            if (muzzleFlash != null)
            {
                muzzleFlash.Emit(flashParticlesCount);
            }

            if (audioSource != null && shootSound != null)
            {
                audioSource.PlayOneShot(shootSound);
            }

            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }
    }
}