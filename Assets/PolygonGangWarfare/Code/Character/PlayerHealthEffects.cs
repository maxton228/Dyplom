using UnityEngine;
using UnityEngine.Rendering;

public class PlayerHealthEffects : MonoBehaviour
{
    private Health health;
    public Volume healthVolume;
    public AudioSource heartAudioSource;

    void Awake()
    {
        health = GetComponent<Health>();
        health.OnDeath += HandlePlayerDeath;
    }

    void Update()
    {
        if (health.isDead) return;

        float healthPercent = health.currentHealth / health.maxHealth;
        float painFactor = Mathf.InverseLerp(0.7f, 0.1f, healthPercent);
        if (healthVolume != null) healthVolume.weight = painFactor;

    }

    void HandlePlayerDeath()
    {
        if (healthVolume != null) healthVolume.weight = 1f;
        if (heartAudioSource != null) heartAudioSource.Stop();
    }
}