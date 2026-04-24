using UnityEngine;
using System;
using UnityEngine.Rendering; // Потрібно для Volume

public class Health : MonoBehaviour
{
    [Header("Налаштування")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isPlayer = false;
    public bool isDead { get; private set; }

    [Header("Ефекти гравця (тільки для Player)")]
    public Volume healthVolume;
    public AudioSource heartAudioSource;

    // Події для інших скриптів (наприклад, для TacticalEnemy)
    public event Action OnDeath;
    public event Action<float> OnDamaged;

    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (!isPlayer)
        {
            SetRagdollState(false);
        }
        else
        {
            // Налаштування для гравця при старті
            if (healthVolume != null) healthVolume.weight = 0f;

            if (heartAudioSource != null)
            {
                heartAudioSource.loop = true;
                heartAudioSource.volume = 0f;
                heartAudioSource.Stop(); // Переконуємось, що не грає
            }
        }
    }

    void Update()
    {
        if (isPlayer && !isDead)
        {
            UpdatePlayerEffects();
        }
    }

    private void UpdatePlayerEffects()
    {
        float healthPercent = currentHealth / maxHealth;

        // Візуальний ефект Vignette
        if (healthVolume != null)
        {
            float painFactor = Mathf.InverseLerp(0.7f, 0.1f, healthPercent);
            healthVolume.weight = painFactor;
        }

        // Логіка звуку серця
        if (heartAudioSource != null)
        {
            if (currentHealth <= 30f && currentHealth > 0)
            {
                if (!heartAudioSource.isPlaying) heartAudioSource.Play();

                float heartFactor = Mathf.InverseLerp(30f, 0f, currentHealth);
                float quietMaxVolume = 0.5f;
                heartAudioSource.volume = Mathf.MoveTowards(heartAudioSource.volume, heartFactor * quietMaxVolume, Time.deltaTime * 2f);
            }
            else
            {
                heartAudioSource.volume = Mathf.MoveTowards(heartAudioSource.volume, 0f, Time.deltaTime * 2f);
                if (heartAudioSource.volume <= 0.01f && heartAudioSource.isPlaying) heartAudioSource.Stop();
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        OnDamaged?.Invoke(amount);

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (isPlayer)
        {
            if (healthVolume != null) healthVolume.weight = 1f;
            if (heartAudioSource != null) heartAudioSource.Stop();
            Debug.Log("<color=red>Гравця вбито!</color>");
        }
        else
        {
            SetRagdollState(true);
        }

        OnDeath?.Invoke();
        Debug.Log(gameObject.name + " помер.");
    }

    // Метод керування Ragdoll для ворогів
    private void SetRagdollState(bool active)
    {
        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();
        foreach (var rb in bodies)
        {
            if (rb.gameObject != gameObject)
            {
                rb.isKinematic = !active;
            }
        }

        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = !active;

        if (TryGetComponent<Collider>(out var mainCol)) mainCol.enabled = !active;
    }

    public void InitHealth(float value)
    {
        maxHealth = value;
        currentHealth = value;
    }
}